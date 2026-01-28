using System.Xml.Serialization;

namespace AydinWyldePortfolioX.Models
{
    [XmlRoot("AdminCredentials")]
    public class AdminCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsInitialized { get; set; } = false;
    }

    [XmlRoot("AdminSession")]
    public class AdminSession
    {
        public string SessionToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class PasswordResetRequest
    {
        public string ResetToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool UsedViaEmail { get; set; }
        public bool UsedViaSms { get; set; }
    }
}
