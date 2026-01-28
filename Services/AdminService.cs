using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using AydinWyldePortfolioX.Models;

namespace AydinWyldePortfolioX.Services
{
    public interface IAdminService
    {
        bool IsAdminInitialized();
        bool InitializeAdmin(string username, string password, string email, string phoneNumber);
        bool ValidateCredentials(string username, string password);
        string GenerateSessionToken(string username);
        bool ValidateSessionToken(string token);
        string GeneratePasswordResetToken(string username);
        bool ValidateResetToken(string token);
        bool ResetPassword(string token, string newPassword);
        AdminCredentials? GetAdminInfo();
        bool UpdateAdminInfo(string email, string phoneNumber);
    }

    public class AdminService : IAdminService
    {
        private readonly string _dataPath;
        private readonly string _credentialsFile;
        private readonly string _sessionsFile;
        private readonly string _resetTokensFile;

        public AdminService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "App_Data", "Secure");
            _credentialsFile = Path.Combine(_dataPath, "admin_credentials.xml");
            _sessionsFile = Path.Combine(_dataPath, "admin_sessions.xml");
            _resetTokensFile = Path.Combine(_dataPath, "reset_tokens.xml");
            
            // Ensure directory exists
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        public bool IsAdminInitialized()
        {
            if (!File.Exists(_credentialsFile)) return false;
            
            var credentials = LoadCredentials();
            return credentials?.IsInitialized ?? false;
        }

        public bool InitializeAdmin(string username, string password, string email, string phoneNumber)
        {
            if (IsAdminInitialized()) return false;

            var salt = GenerateSalt();
            var passwordHash = HashPassword(password, salt);

            var credentials = new AdminCredentials
            {
                Username = username,
                PasswordHash = passwordHash,
                Salt = salt,
                Email = email,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsInitialized = true
            };

            SaveCredentials(credentials);
            return true;
        }

        public bool ValidateCredentials(string username, string password)
        {
            var credentials = LoadCredentials();
            if (credentials == null || !credentials.IsInitialized) return false;

            if (!string.Equals(credentials.Username, username, StringComparison.OrdinalIgnoreCase))
                return false;

            var passwordHash = HashPassword(password, credentials.Salt);
            return passwordHash == credentials.PasswordHash;
        }

        public string GenerateSessionToken(string username)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var session = new AdminSession
            {
                SessionToken = token,
                Username = username,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            var sessions = LoadSessions();
            sessions.RemoveAll(s => s.Username == username || s.ExpiresAt < DateTime.UtcNow);
            sessions.Add(session);
            SaveSessions(sessions);

            return token;
        }

        public bool ValidateSessionToken(string token)
        {
            var sessions = LoadSessions();
            var session = sessions.FirstOrDefault(s => s.SessionToken == token && s.ExpiresAt > DateTime.UtcNow);
            return session != null;
        }

        public string GeneratePasswordResetToken(string username)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var request = new PasswordResetRequest
            {
                ResetToken = token,
                Username = username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            var requests = LoadResetTokens();
            requests.RemoveAll(r => r.ExpiresAt < DateTime.UtcNow);
            requests.Add(request);
            SaveResetTokens(requests);

            return token;
        }

        public bool ValidateResetToken(string token)
        {
            var requests = LoadResetTokens();
            return requests.Any(r => r.ResetToken == token && r.ExpiresAt > DateTime.UtcNow);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var requests = LoadResetTokens();
            var request = requests.FirstOrDefault(r => r.ResetToken == token && r.ExpiresAt > DateTime.UtcNow);
            
            if (request == null) return false;

            var credentials = LoadCredentials();
            if (credentials == null) return false;

            var newSalt = GenerateSalt();
            credentials.PasswordHash = HashPassword(newPassword, newSalt);
            credentials.Salt = newSalt;
            credentials.LastModified = DateTime.UtcNow;

            SaveCredentials(credentials);

            // Remove used token
            requests.Remove(request);
            SaveResetTokens(requests);

            return true;
        }

        public AdminCredentials? GetAdminInfo()
        {
            return LoadCredentials();
        }

        public bool UpdateAdminInfo(string email, string phoneNumber)
        {
            var credentials = LoadCredentials();
            if (credentials == null) return false;

            credentials.Email = email;
            credentials.PhoneNumber = phoneNumber;
            credentials.LastModified = DateTime.UtcNow;

            SaveCredentials(credentials);
            return true;
        }

        private string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        private AdminCredentials? LoadCredentials()
        {
            if (!File.Exists(_credentialsFile)) return null;

            try
            {
                var serializer = new XmlSerializer(typeof(AdminCredentials));
                using var stream = File.OpenRead(_credentialsFile);
                return (AdminCredentials?)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
        }

        private void SaveCredentials(AdminCredentials credentials)
        {
            var serializer = new XmlSerializer(typeof(AdminCredentials));
            using var stream = File.Create(_credentialsFile);
            serializer.Serialize(stream, credentials);
        }

        private List<AdminSession> LoadSessions()
        {
            if (!File.Exists(_sessionsFile)) return new List<AdminSession>();

            try
            {
                var serializer = new XmlSerializer(typeof(List<AdminSession>));
                using var stream = File.OpenRead(_sessionsFile);
                return (List<AdminSession>?)serializer.Deserialize(stream) ?? new List<AdminSession>();
            }
            catch
            {
                return new List<AdminSession>();
            }
        }

        private void SaveSessions(List<AdminSession> sessions)
        {
            var serializer = new XmlSerializer(typeof(List<AdminSession>));
            using var stream = File.Create(_sessionsFile);
            serializer.Serialize(stream, sessions);
        }

        private List<PasswordResetRequest> LoadResetTokens()
        {
            if (!File.Exists(_resetTokensFile)) return new List<PasswordResetRequest>();

            try
            {
                var serializer = new XmlSerializer(typeof(List<PasswordResetRequest>));
                using var stream = File.OpenRead(_resetTokensFile);
                return (List<PasswordResetRequest>?)serializer.Deserialize(stream) ?? new List<PasswordResetRequest>();
            }
            catch
            {
                return new List<PasswordResetRequest>();
            }
        }

        private void SaveResetTokens(List<PasswordResetRequest> tokens)
        {
            var serializer = new XmlSerializer(typeof(List<PasswordResetRequest>));
            using var stream = File.Create(_resetTokensFile);
            serializer.Serialize(stream, tokens);
        }
    }
}
