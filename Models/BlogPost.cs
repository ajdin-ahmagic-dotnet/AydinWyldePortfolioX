using System.Xml.Serialization;

namespace AydinWyldePortfolioX.Models
{
    [XmlRoot("BlogPost")]
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        public List<string> Tags { get; set; } = new List<string>();
        
        public bool IsFeatured { get; set; }
        public string FeaturedImage { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public class BlogCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int PostCount { get; set; }
    }
}