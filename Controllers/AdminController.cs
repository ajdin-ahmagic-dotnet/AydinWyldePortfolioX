using Microsoft.AspNetCore.Mvc;
using AydinWyldePortfolioX.Models;
using AydinWyldePortfolioX.Services;

namespace AydinWyldePortfolioX.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IBlogService _blogService;
        private readonly IVisitorTrackingService _visitorService;
        private readonly INotificationService _notificationService;

        public AdminController(
            IAdminService adminService,
            IBlogService blogService,
            IVisitorTrackingService visitorService,
            INotificationService notificationService)
        {
            _adminService = adminService;
            _blogService = blogService;
            _visitorService = visitorService;
            _notificationService = notificationService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var isInitialized = _adminService.IsAdminInitialized();
            return Json(new { initialized = isInitialized });
        }

        [HttpPost("initialize")]
        public IActionResult Initialize([FromBody] AdminInitializeModel model)
        {
            if (_adminService.IsAdminInitialized())
            {
                return BadRequest(new { error = "Admin already initialized" });
            }

            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "Username and password are required" });
            }

            if (model.Password.Length < 8)
            {
                return BadRequest(new { error = "Password must be at least 8 characters" });
            }

            var success = _adminService.InitializeAdmin(
                model.Username, 
                model.Password, 
                model.Email ?? "", 
                model.PhoneNumber ?? "");

            if (success)
            {
                var token = _adminService.GenerateSessionToken(model.Username);
                Response.Cookies.Append("AdminSession", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });
                return Ok(new { success = true, message = "Admin account created successfully" });
            }

            return BadRequest(new { error = "Failed to initialize admin" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (!_adminService.IsAdminInitialized())
            {
                return BadRequest(new { error = "Admin not initialized", needsSetup = true });
            }

            if (_adminService.ValidateCredentials(model.Username, model.Password))
            {
                var token = _adminService.GenerateSessionToken(model.Username);
                Response.Cookies.Append("AdminSession", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });
                return Ok(new { success = true });
            }

            return Unauthorized(new { error = "Invalid credentials" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AdminSession");
            return Ok(new { success = true });
        }

        [HttpGet("check")]
        public IActionResult CheckSession()
        {
            var token = Request.Cookies["AdminSession"];
            if (string.IsNullOrEmpty(token) || !_adminService.ValidateSessionToken(token))
            {
                return Unauthorized(new { authenticated = false });
            }
            return Ok(new { authenticated = true });
        }

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetRequestModel model)
        {
            var admin = _adminService.GetAdminInfo();
            if (admin == null)
            {
                return BadRequest(new { error = "Admin not found" });
            }

            var resetToken = _adminService.GeneratePasswordResetToken(admin.Username);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (model.Method == "email" && !string.IsNullOrEmpty(admin.Email))
            {
                var resetUrl = $"{baseUrl}/admin/reset-password";
                await _notificationService.SendPasswordResetEmail(admin.Email, resetToken, resetUrl);
                return Ok(new { success = true, message = "Reset link sent to your email" });
            }
            else if (model.Method == "sms" && !string.IsNullOrEmpty(admin.PhoneNumber))
            {
                var code = resetToken.Substring(0, 6).ToUpper();
                await _notificationService.SendPasswordResetSms(admin.PhoneNumber, code);
                return Ok(new { success = true, message = "Reset code sent to your phone" });
            }

            return BadRequest(new { error = "No valid contact method configured" });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest(new { error = "Token and new password are required" });
            }

            if (model.NewPassword.Length < 8)
            {
                return BadRequest(new { error = "Password must be at least 8 characters" });
            }

            if (_adminService.ResetPassword(model.Token, model.NewPassword))
            {
                return Ok(new { success = true, message = "Password reset successfully" });
            }

            return BadRequest(new { error = "Invalid or expired reset token" });
        }

        // Dashboard endpoints
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var data = _visitorService.GetDashboardData();
            return Json(data);
        }

        // Blog management endpoints
        [HttpGet("blog/posts")]
        public IActionResult GetBlogPosts()
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var posts = _blogService.GetAllPosts();
            return Json(posts);
        }

        [HttpGet("blog/posts/{id}")]
        public IActionResult GetBlogPost(int id)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized();
            }

            var post = _blogService.GetPostById(id);
            if (post == null)
            {
                return NotFound();
            }
            return Json(post);
        }

        [HttpPost("blog/posts")]
        public IActionResult CreateBlogPost([FromBody] BlogPostInputModel post)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized(new { error = "Unauthorized - please log in again", code = 4001 });
            }

            try
            {
                if (post == null)
                {
                    return BadRequest(new { error = "Invalid payload", code = 4003 });
                }

                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    return BadRequest(new { error = "Title is required", code = 4002 });
                }

                var mapped = post.ToBlogPost();

                if (_blogService.CreatePost(mapped))
                {
                    return Ok(new { success = true, message = "Post created successfully", postId = mapped.Id });
                }
                return StatusCode(500, new { error = "Failed to create post - check server logs", code = 5001 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] CreateBlogPost error: {ex.Message}");
                return StatusCode(500, new { error = $"Server error: {ex.Message}", code = 5002 });
            }
        }

        [HttpPut("blog/posts/{id}")]
        public IActionResult UpdateBlogPost(int id, [FromBody] BlogPostInputModel post)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized(new { error = "Unauthorized - please log in again", code = 4001 });
            }

            try
            {
                if (post == null)
                {
                    return BadRequest(new { error = "Invalid payload", code = 4003 });
                }

                if (string.IsNullOrWhiteSpace(post.Title))
                {
                    return BadRequest(new { error = "Title is required", code = 4002 });
                }

                var mapped = post.ToBlogPost();
                mapped.Id = id;
                if (_blogService.UpdatePost(mapped))
                {
                    return Ok(new { success = true, message = "Post updated successfully" });
                }
                return StatusCode(500, new { error = "Failed to update post - check server logs", code = 5001 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] UpdateBlogPost error: {ex.Message}");
                return StatusCode(500, new { error = $"Server error: {ex.Message}", code = 5002 });
            }
        }

        [HttpDelete("blog/posts/{id}")]
        public IActionResult DeleteBlogPost(int id)
        {
            if (!IsAuthenticated())
            {
                return Unauthorized(new { error = "Unauthorized - please log in again", code = 4001 });
            }

            try
            {
                if (_blogService.DeletePost(id))
                {
                    return Ok(new { success = true, message = "Post deleted successfully" });
                }
                return StatusCode(500, new { error = "Failed to delete post - check server logs", code = 5001 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminController] DeleteBlogPost error: {ex.Message}");
                return StatusCode(500, new { error = $"Server error: {ex.Message}", code = 5002 });
            }
        }

        [HttpGet("blog/categories")]
        public IActionResult GetCategories()
        {
            var categories = _blogService.GetAllCategories();
            return Json(categories);
        }

        private bool IsAuthenticated()
        {
            var token = Request.Cookies["AdminSession"];
            return !string.IsNullOrEmpty(token) && _adminService.ValidateSessionToken(token);
        }
    }

    // Request models
    public class AdminInitializeModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ResetRequestModel
    {
        public string Method { get; set; } = string.Empty; // "email" or "sms"
    }

    public class ResetPasswordModel
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class BlogPostInputModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsFeatured { get; set; }
        public string FeaturedImage { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;

        public BlogPost ToBlogPost() => new BlogPost
        {
            Id = Id ?? 0,
            Title = Title ?? string.Empty,
            Summary = Summary ?? string.Empty,
            Content = Content ?? string.Empty,
            Author = Author ?? string.Empty,
            Category = Category ?? string.Empty,
            Tags = Tags ?? new List<string>(),
            IsFeatured = IsFeatured,
            FeaturedImage = FeaturedImage ?? string.Empty,
            IsPublished = IsPublished,
            Slug = string.Empty
        };
    }
}
