using System.Xml.Serialization;
using AydinWyldePortfolioX.Models;

namespace AydinWyldePortfolioX.Services
{
    public interface IBlogService
    {
        List<BlogPost> GetAllPosts();
        List<BlogPost> GetPostsByCategory(string category);
        List<BlogPost> SearchPosts(string query);
        BlogPost? GetPostById(int id);
        BlogPost? GetPostBySlug(string slug);
        bool CreatePost(BlogPost post);
        bool UpdatePost(BlogPost post);
        bool DeletePost(int id);
        List<string> GetAllCategories();
        List<string> GetAllTags();
        List<BlogPost> GetLatestPosts(int count = 5);
        List<BlogPost> GetFeaturedPosts();
    }

    public class BlogService : IBlogService
    {
        private readonly string _dataPath;
        private readonly string _postsFile;
        private static readonly object _lock = new object();

        public BlogService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "App_Data", "Blog");
            _postsFile = Path.Combine(_dataPath, "blog_posts.xml");

            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }

            // Initialize with sample data if empty
            if (!File.Exists(_postsFile))
            {
                InitializeSampleData();
                Console.WriteLine("This ran with sample data, blog posts xml created");
                Console.Write("Blog posts xml created at: " + _postsFile);
                Console.WriteLine();
                int A = 0; // Placeholder to set a breakpoint here if needed
                var B = A;
                string? C = "This is a debug string";
                if (C.Length > 0)
                {
                    B += C.Length; 
                    // typing is not working in watch window? ddd
                }
            }
        }

        public List<BlogPost> GetAllPosts()
        {
            return LoadPosts().OrderByDescending(p => p.PublishDate).ToList();
        }

        public List<BlogPost> GetPostsByCategory(string category)
        {
            return LoadPosts()
                .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PublishDate)
                .ToList();
        }

        public List<BlogPost> SearchPosts(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return GetAllPosts();

            var lowerQuery = query.ToLower();
            return LoadPosts()
                .Where(p => p.Title.ToLower().Contains(lowerQuery) ||
                           p.Content.ToLower().Contains(lowerQuery) ||
                           p.Summary.ToLower().Contains(lowerQuery) ||
                           p.Tags.Any(t => t.ToLower().Contains(lowerQuery)) ||
                           p.Category.ToLower().Contains(lowerQuery))
                .OrderByDescending(p => p.PublishDate)
                .ToList();
        }

        public BlogPost? GetPostById(int id)
        {
            return LoadPosts().FirstOrDefault(p => p.Id == id);
        }

        public BlogPost? GetPostBySlug(string slug)
        {
            return LoadPosts().FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }

        public bool CreatePost(BlogPost post)
        {
            lock (_lock)
            {
                try
                {
                    EnsureDirectoryExists();
                    var posts = LoadPosts();
                    post.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
                    post.PublishDate = DateTime.UtcNow;
                    post.LastModified = DateTime.UtcNow;
                    post.Slug = GenerateSlug(post.Title);
                    post.Author = post.Author ?? "Aydin Wylde";
                    posts.Add(post);
                    SavePosts(posts);
                    Console.WriteLine($"[BlogService] Created post ID {post.Id}: {post.Title}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BlogService] ERROR creating post: {ex.Message}");
                    Console.WriteLine($"[BlogService] Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public bool UpdatePost(BlogPost post)
        {
            lock (_lock)
            {
                try
                {
                    EnsureDirectoryExists();
                    var posts = LoadPosts();
                    var existing = posts.FirstOrDefault(p => p.Id == post.Id);
                    if (existing == null)
                    {
                        Console.WriteLine($"[BlogService] Post ID {post.Id} not found for update");
                        return false;
                    }

                    // Preserve original data
                    post.PublishDate = existing.PublishDate;
                    post.Author = existing.Author ?? "Aydin Wylde";
                    post.ViewCount = existing.ViewCount;

                    posts.Remove(existing);
                    post.LastModified = DateTime.UtcNow;
                    if (string.IsNullOrEmpty(post.Slug))
                    {
                        post.Slug = GenerateSlug(post.Title);
                    }
                    posts.Add(post);
                    SavePosts(posts);
                    Console.WriteLine($"[BlogService] Updated post ID {post.Id}: {post.Title}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BlogService] ERROR updating post: {ex.Message}");
                    Console.WriteLine($"[BlogService] Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public bool DeletePost(int id)
        {
            lock (_lock)
            {
                try
                {
                    var posts = LoadPosts();
                    var post = posts.FirstOrDefault(p => p.Id == id);
                    if (post == null)
                    {
                        Console.WriteLine($"[BlogService] Post ID {id} not found for deletion");
                        return false;
                    }

                    posts.Remove(post);
                    SavePosts(posts);
                    Console.WriteLine($"[BlogService] Deleted post ID {id}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BlogService] ERROR deleting post: {ex.Message}");
                    Console.WriteLine($"[BlogService] Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
                Console.WriteLine($"[BlogService] Created directory: {_dataPath}");
            }
        }

        public List<string> GetAllCategories()
        {
            return LoadPosts()
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public List<string> GetAllTags()
        {
            return LoadPosts()
                .SelectMany(p => p.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        public List<BlogPost> GetLatestPosts(int count = 5)
        {
            return LoadPosts()
                .OrderByDescending(p => p.PublishDate)
                .Take(count)
                .ToList();
        }

        public List<BlogPost> GetFeaturedPosts()
        {
            return LoadPosts()
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.PublishDate)
                .ToList();
        }

        private string GenerateSlug(string title)
        {
            var slug = title.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("#", "sharp");

            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            return slug.Trim('-');
        }

        private List<BlogPost> LoadPosts()
        {
            if (!File.Exists(_postsFile)) return new List<BlogPost>();

            try
            {
                var serializer = new XmlSerializer(typeof(List<BlogPost>));
                using var stream = File.OpenRead(_postsFile);
                return (List<BlogPost>?)serializer.Deserialize(stream) ?? new List<BlogPost>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlogService] ERROR loading posts: {ex.Message}");
                return new List<BlogPost>();
            }
        }

        private void SavePosts(List<BlogPost> posts)
        {
            try
            {
                EnsureDirectoryExists();
                var serializer = new XmlSerializer(typeof(List<BlogPost>));
                
                // Write to temp file first, then rename for atomic operation
                var tempFile = _postsFile + ".tmp";
                using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    serializer.Serialize(stream, posts);
                    stream.Flush(true);
                }

                // On Windows, File.Replace is atomic and avoids partial writes.
                if (File.Exists(_postsFile))
                {
                    File.Replace(tempFile, _postsFile, null);
                }
                else
                {
                    File.Move(tempFile, _postsFile);
                }
                
                Console.WriteLine($"[BlogService] Saved {posts.Count} posts to {_postsFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlogService] CRITICAL ERROR saving posts: {ex.Message}");
                Console.WriteLine($"[BlogService] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to signal failure to caller
            }
        }

        private void InitializeSampleData()
        {
            var samplePosts = new List<BlogPost>
            {
                new BlogPost
                {
                    Id = 1,
                    Title = "Getting Started with ASP.NET Core 9",
                    Slug = "getting-started-aspnet-core-9",
                    Summary = "A comprehensive guide to building modern web applications with ASP.NET Core 9, covering the latest features and best practices.",
                    Content = @"<h3>Introduction</h3>
<p>ASP.NET Core 9 brings exciting new features and performance improvements to the .NET ecosystem. In this article, we'll explore the key changes and how to leverage them in your applications.</p>

<h3>What's New in .NET 9</h3>
<p>The latest release focuses on performance, cloud-native development, and developer productivity. Key highlights include:</p>
<ul>
<li>Native AOT improvements for faster startup times</li>
<li>Enhanced minimal APIs with better OpenAPI support</li>
<li>Blazor improvements for hybrid applications</li>
<li>Better performance in Entity Framework Core</li>
</ul>

<h3>Getting Started</h3>
<p>To create a new ASP.NET Core 9 project, ensure you have the .NET 9 SDK installed, then run:</p>
<pre><code>dotnet new webapp -n MyWebApp
cd MyWebApp
dotnet run</code></pre>

<h3>Conclusion</h3>
<p>ASP.NET Core 9 continues to evolve as a powerful framework for building modern web applications. Start exploring today!</p>",
                    PublishDate = DateTime.UtcNow.AddDays(-2),
                    Author = "Aydin Wylde",
                    Category = "ASP.NET Core",
                    Tags = new List<string> { "ASP.NET Core", ".NET 9", "Web Development", "Tutorial" },
                    IsFeatured = true,
                    FeaturedImage = "/images/blog/aspnet-core-9.jpg"
                },
                new BlogPost
                {
                    Id = 2,
                    Title = "Mastering C# Pattern Matching",
                    Slug = "mastering-csharp-pattern-matching",
                    Summary = "Deep dive into C# pattern matching features, from basic is expressions to advanced switch expressions and recursive patterns.",
                    Content = @"<h3>Pattern Matching in C#</h3>
<p>Pattern matching is one of the most powerful features in modern C#. It allows you to write cleaner, more expressive code by testing values against patterns.</p>

<h3>Type Patterns</h3>
<pre><code>object obj = ""Hello"";
if (obj is string s)
{
    Console.WriteLine(s.ToUpper());
}</code></pre>

<h3>Switch Expressions</h3>
<pre><code>var discount = customer switch
{
    { IsPremium: true, Years: > 5 } => 0.20m,
    { IsPremium: true } => 0.15m,
    { Years: > 3 } => 0.10m,
    _ => 0.05m
};</code></pre>

<h3>List Patterns (C# 11+)</h3>
<pre><code>int[] numbers = { 1, 2, 3 };
var result = numbers switch
{
    [1, 2, 3] => ""Exact match"",
    [1, .., 3] => ""Starts with 1, ends with 3"",
    [_, _, _] => ""Three elements"",
    _ => ""Other""
};</code></pre>",
                    PublishDate = DateTime.UtcNow.AddDays(-5),
                    Author = "Aydin Wylde",
                    Category = "C#",
                    Tags = new List<string> { "C#", "Pattern Matching", "Best Practices" },
                    IsFeatured = true,
                    FeaturedImage = "/images/blog/csharp-patterns.jpg"
                },
                new BlogPost
                {
                    Id = 3,
                    Title = "Building a Career in Software Development",
                    Slug = "building-career-software-development",
                    Summary = "Personal insights and lessons learned from my journey as a software developer, including tips for aspiring developers.",
                    Content = @"<h3>My Journey</h3>
<p>After years in the software development industry, I've gathered valuable insights that I wish someone had shared with me when I started.</p>

<h3>Key Lessons Learned</h3>
<ul>
<li><strong>Never stop learning:</strong> Technology evolves rapidly. Dedicate time each week to learning new skills.</li>
<li><strong>Build a portfolio:</strong> Personal projects demonstrate your abilities better than any resume.</li>
<li><strong>Soft skills matter:</strong> Communication and teamwork are as important as coding skills.</li>
<li><strong>Embrace failure:</strong> Every bug is a learning opportunity. Don't fear making mistakes.</li>
</ul>

<h3>Advice for Beginners</h3>
<p>Start with fundamentals before jumping to frameworks. Understanding how things work under the hood makes you a better developer.</p>",
                    PublishDate = DateTime.UtcNow.AddDays(-10),
                    Author = "Aydin Wylde",
                    Category = "Personal",
                    Tags = new List<string> { "Career", "Personal", "Advice", "Development" },
                    IsFeatured = false,
                    FeaturedImage = "/images/blog/career.jpg"
                },
                new BlogPost
                {
                    Id = 4,
                    Title = ".NET 9 Performance Improvements",
                    Slug = "dotnet-9-performance-improvements",
                    Summary = "Exploring the significant performance enhancements in .NET 9, including JIT optimizations and runtime improvements.",
                    Content = @"<h3>Performance in .NET 9</h3>
<p>.NET 9 brings substantial performance improvements across the board. Let's explore the key enhancements.</p>

<h3>JIT Compiler Improvements</h3>
<p>The JIT compiler in .NET 9 includes better loop optimizations, improved inlining decisions, and enhanced generic code generation.</p>

<h3>Native AOT</h3>
<p>Native AOT compilation continues to improve, offering faster startup times and smaller deployment sizes for console and server applications.</p>

<h3>Benchmarks</h3>
<p>Internal benchmarks show 10-20% performance improvements in common scenarios compared to .NET 8.</p>",
                    PublishDate = DateTime.UtcNow.AddDays(-15),
                    Author = "Aydin Wylde",
                    Category = "News",
                    Tags = new List<string> { ".NET 9", "Performance", "News" },
                    IsFeatured = true,
                    FeaturedImage = "/images/blog/dotnet-performance.jpg"
                },
                new BlogPost
                {
                    Id = 5,
                    Title = "Entity Framework Core Best Practices",
                    Slug = "entity-framework-core-best-practices",
                    Summary = "Essential tips and patterns for using Entity Framework Core efficiently in production applications.",
                    Content = @"<h3>EF Core Best Practices</h3>
<p>Entity Framework Core is a powerful ORM, but using it incorrectly can lead to performance issues. Here are essential practices.</p>

<h3>Use AsNoTracking for Read-Only Queries</h3>
<pre><code>var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.IsActive)
    .ToListAsync();</code></pre>

<h3>Eager Loading vs Lazy Loading</h3>
<p>Prefer explicit eager loading with Include() over lazy loading to avoid N+1 query problems.</p>

<h3>Projection for Better Performance</h3>
<pre><code>var productDtos = await _context.Products
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name
    })
    .ToListAsync();</code></pre>",
                    PublishDate = DateTime.UtcNow.AddDays(-20),
                    Author = "Aydin Wylde",
                    Category = "ASP.NET Core",
                    Tags = new List<string> { "Entity Framework", "Database", "Performance", "Best Practices" },
                    IsFeatured = false,
                    FeaturedImage = "/images/blog/efcore.jpg"
                }
            };

            SavePosts(samplePosts);
        }
    }
}
