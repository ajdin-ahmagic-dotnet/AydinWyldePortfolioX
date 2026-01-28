using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AydinWyldePortfolioX.Models;
using AydinWyldePortfolioX.Services;
using Microsoft.AspNetCore.Mvc;
using DXApplication3.Models;

namespace AydinWyldePortfolioX.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogService _blogService;

        public HomeController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public IActionResult Index()
        {
            // Get latest blog posts for hero section
            ViewBag.LatestPosts = _blogService.GetLatestPosts(3);
            ViewBag.FeaturedPosts = _blogService.GetFeaturedPosts().Take(3).ToList();
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
        
        public IActionResult Blog(int page = 1, string category = null, string search = null)
        {
            const int pageSize = 6;
            
            List<BlogPost> posts;
            if (!string.IsNullOrEmpty(search))
            {
                posts = _blogService.SearchPosts(search);
                ViewBag.SearchQuery = search;
            }
            else if (!string.IsNullOrEmpty(category))
            {
                posts = _blogService.GetPostsByCategory(category);
                ViewBag.SelectedCategory = category;
            }
            else
            {
                posts = _blogService.GetAllPosts();
            }

            var totalPosts = posts.Count;
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
            
            var pagedPosts = posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = _blogService.GetAllCategories();
            ViewBag.FeaturedPosts = _blogService.GetFeaturedPosts().Take(3).ToList();
            
            return View(pagedPosts);
        }

        public IActionResult BlogPost(string slug)
        {
            var post = _blogService.GetPostBySlug(slug);
            if (post == null)
            {
                return NotFound();
            }
            
            ViewBag.RecentPosts = _blogService.GetLatestPosts(5).Where(p => p.Id != post.Id).Take(4).ToList();
            ViewBag.RelatedPosts = _blogService.GetPostsByCategory(post.Category)
                .Where(p => p.Id != post.Id)
                .Take(3)
                .ToList();
            
            return View(post);
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
