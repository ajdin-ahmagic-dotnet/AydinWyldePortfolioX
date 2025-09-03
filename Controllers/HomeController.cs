using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AydinWyldePortfolioX.Models;
using Microsoft.AspNetCore.Mvc;
using DXApplication3.Models;

namespace AydinWyldePortfolioX.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        // Accept optional id to show individual skill pages at /Home/Skills/{id}
        public IActionResult Skills(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.UseParticles = false;
                return View();
            }

            // When showing a specific skill page, enable particles background
            ViewBag.UseParticles = true;

            // Normalize id to match view file names (capitalize first letter)
            var name = id.Trim().ToLower();
            var viewName = name.Length > 0 ? char.ToUpperInvariant(name[0]) + name.Substring(1) : name;

            // Try to return view from Views/Home/Skills/{ViewName}.cshtml
            return View($"Skills/{viewName}");
        }

        public IActionResult Projects()
        {
            return View();
        }

        public IActionResult Education()
        {
            return View();
        }
        
        public IActionResult Blog(int page = 1)
        {
            // Generate sample blog posts for demo
            var blogPosts = new List<BlogPost>
            {
                new BlogPost
                {
                    Id = 1,
                    Title = "Microservices Architecture with .NET 9",
                    Content = "Exploring the latest microservices patterns and best practices with .NET 9...",
                    PublishDate = DateTime.Now.AddDays(-5),
                    Author = "Aydin Wylde",
                    Category = "Architecture",
                    Tags = new List<string> { "Microservices", ".NET 9", "Architecture" }
                },
                new BlogPost
                {
                    Id = 2,
                    Title = "Optimizing Entity Framework Core Performance",
                    Content = "Tips and tricks to boost your EF Core queries and improve application performance...",
                    PublishDate = DateTime.Now.AddDays(-12),
                    Author = "Aydin Wylde",
                    Category = "Database",
                    Tags = new List<string> { "EF Core", "Performance", "SQL" }
                },
                new BlogPost
                {
                    Id = 3,
                    Title = "Building Secure APIs with ASP.NET Core",
                    Content = "Security best practices for your Web APIs to protect against common vulnerabilities...",
                    PublishDate = DateTime.Now.AddDays(-18),
                    Author = "Aydin Wylde",
                    Category = "Security",
                    Tags = new List<string> { "API", "Security", "ASP.NET Core" }
                }
            };
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = 1;
            
            return View(blogPosts);
        }

        public IActionResult Search(string q)
        {
            // Pass the query to the View via ViewBag
            ViewBag.SearchQuery = q;
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
