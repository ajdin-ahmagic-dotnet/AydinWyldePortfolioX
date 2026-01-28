using System.Xml.Serialization;

namespace AydinWyldePortfolioX.Models
{
    [XmlRoot("VisitorStats")]
    public class VisitorStats
    {
        [XmlArray("Visits")]
        [XmlArrayItem("Visit")]
        public List<VisitorEntry> Visits { get; set; } = new List<VisitorEntry>();
        
        [XmlArray("DailyStats")]
        [XmlArrayItem("Day")]
        public List<DailyStats> DailyStats { get; set; } = new List<DailyStats>();
    }

    public class VisitorEntry
    {
        public string SessionId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string PageVisited { get; set; } = string.Empty;
        public string Referrer { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
    }

    public class DailyStats
    {
        public DateTime Date { get; set; }
        public int TotalVisits { get; set; }
        public int UniqueVisitors { get; set; }
        public int PageViews { get; set; }
        public double AverageSessionDuration { get; set; }
        public double BounceRate { get; set; }
    }

    public class DashboardViewModel
    {
        public int TodayVisitors { get; set; }
        public int TotalVisitors { get; set; }
        public int ThisWeekVisitors { get; set; }
        public int ThisMonthVisitors { get; set; }
        public List<PageViewStats> TopPages { get; set; } = new List<PageViewStats>();
        public List<BrowserStats> BrowserStats { get; set; } = new List<BrowserStats>();
        public List<DailyStats> Last30Days { get; set; } = new List<DailyStats>();
        public List<VisitorEntry> RecentVisitors { get; set; } = new List<VisitorEntry>();
    }

    public class PageViewStats
    {
        public string PageName { get; set; } = string.Empty;
        public int Views { get; set; }
        public double Percentage { get; set; }
    }

    public class BrowserStats
    {
        public string BrowserName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
