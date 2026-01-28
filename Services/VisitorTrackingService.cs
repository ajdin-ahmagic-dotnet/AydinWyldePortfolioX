using System.Xml.Serialization;
using AydinWyldePortfolioX.Models;

namespace AydinWyldePortfolioX.Services
{
    public interface IVisitorTrackingService
    {
        void TrackVisit(HttpContext context, string page);
        DashboardViewModel GetDashboardData();
        List<VisitorEntry> GetRecentVisitors(int count = 50);
        VisitorStats GetAllStats();
    }

    public class VisitorTrackingService : IVisitorTrackingService
    {
        private readonly string _dataPath;
        private readonly string _statsFile;
        private static readonly object _lock = new object();

        public VisitorTrackingService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "App_Data", "Analytics");
            _statsFile = Path.Combine(_dataPath, "visitor_stats.xml");

            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        public void TrackVisit(HttpContext context, string page)
        {
            var entry = new VisitorEntry
            {
                SessionId = context.Session?.Id ?? Guid.NewGuid().ToString(),
                IpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                PageVisited = page,
                Referrer = context.Request.Headers["Referer"].ToString(),
                VisitTime = DateTime.UtcNow,
                Browser = ParseBrowser(context.Request.Headers["User-Agent"].ToString()),
                OperatingSystem = ParseOS(context.Request.Headers["User-Agent"].ToString()),
                DeviceType = ParseDeviceType(context.Request.Headers["User-Agent"].ToString())
            };

            lock (_lock)
            {
                var stats = LoadStats();
                stats.Visits.Add(entry);

                // Update daily stats
                var today = DateTime.UtcNow.Date;
                var dailyStat = stats.DailyStats.FirstOrDefault(d => d.Date.Date == today);
                if (dailyStat == null)
                {
                    dailyStat = new DailyStats { Date = today };
                    stats.DailyStats.Add(dailyStat);
                }
                dailyStat.TotalVisits++;
                dailyStat.PageViews++;
                
                // Calculate unique visitors for today
                var todayVisits = stats.Visits.Where(v => v.VisitTime.Date == today).ToList();
                dailyStat.UniqueVisitors = todayVisits.Select(v => v.IpAddress).Distinct().Count();

                // Keep only last 90 days of detailed data
                var cutoffDate = DateTime.UtcNow.AddDays(-90);
                stats.Visits.RemoveAll(v => v.VisitTime < cutoffDate);
                stats.DailyStats.RemoveAll(d => d.Date < cutoffDate);

                SaveStats(stats);
            }
        }

        public DashboardViewModel GetDashboardData()
        {
            var stats = LoadStats();
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            var viewModel = new DashboardViewModel
            {
                TodayVisitors = stats.Visits.Count(v => v.VisitTime.Date == today),
                TotalVisitors = stats.Visits.Select(v => v.IpAddress).Distinct().Count(),
                ThisWeekVisitors = stats.Visits.Where(v => v.VisitTime >= weekAgo).Select(v => v.IpAddress).Distinct().Count(),
                ThisMonthVisitors = stats.Visits.Where(v => v.VisitTime >= monthAgo).Select(v => v.IpAddress).Distinct().Count(),
                Last30Days = stats.DailyStats.Where(d => d.Date >= monthAgo).OrderBy(d => d.Date).ToList(),
                RecentVisitors = stats.Visits.OrderByDescending(v => v.VisitTime).Take(20).ToList()
            };

            // Calculate top pages
            var pageGroups = stats.Visits
                .GroupBy(v => v.PageVisited)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToList();

            var totalViews = stats.Visits.Count;
            viewModel.TopPages = pageGroups.Select(g => new PageViewStats
            {
                PageName = g.Key,
                Views = g.Count(),
                Percentage = totalViews > 0 ? Math.Round((double)g.Count() / totalViews * 100, 1) : 0
            }).ToList();

            // Calculate browser stats
            var browserGroups = stats.Visits
                .GroupBy(v => v.Browser)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToList();

            viewModel.BrowserStats = browserGroups.Select(g => new BrowserStats
            {
                BrowserName = g.Key,
                Count = g.Count(),
                Percentage = totalViews > 0 ? Math.Round((double)g.Count() / totalViews * 100, 1) : 0
            }).ToList();

            return viewModel;
        }

        public List<VisitorEntry> GetRecentVisitors(int count = 50)
        {
            var stats = LoadStats();
            return stats.Visits.OrderByDescending(v => v.VisitTime).Take(count).ToList();
        }

        public VisitorStats GetAllStats()
        {
            return LoadStats();
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string ParseBrowser(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            
            if (userAgent.Contains("Edg")) return "Edge";
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari")) return "Safari";
            if (userAgent.Contains("Opera") || userAgent.Contains("OPR")) return "Opera";
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) return "Internet Explorer";
            
            return "Other";
        }

        private string ParseOS(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            
            if (userAgent.Contains("Windows NT 10")) return "Windows 10/11";
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac OS X")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";
            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iOS") || userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
            
            return "Other";
        }

        private string ParseDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            
            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") && !userAgent.Contains("Tablet"))
                return "Mobile";
            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                return "Tablet";
            
            return "Desktop";
        }

        private VisitorStats LoadStats()
        {
            if (!File.Exists(_statsFile)) return new VisitorStats();

            try
            {
                var serializer = new XmlSerializer(typeof(VisitorStats));
                using var stream = File.OpenRead(_statsFile);
                return (VisitorStats?)serializer.Deserialize(stream) ?? new VisitorStats();
            }
            catch
            {
                return new VisitorStats();
            }
        }

        private void SaveStats(VisitorStats stats)
        {
            var serializer = new XmlSerializer(typeof(VisitorStats));
            using var stream = File.Create(_statsFile);
            serializer.Serialize(stream, stats);
        }
    }
}
