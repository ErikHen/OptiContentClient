
namespace OptiContentClient.Models
{
    public class Content
    {
        public string Name { get; set; } = null!;
        public ContentLink ContentLink { get; set; } = null!;
        public string[] ContentType { get; set; } = null!;
        public string RouteSegment { get; set; } = null!;
        public ContentLanguage Language { get; set; } = null!;
        public ContentLanguage[]? ExistingLanguages { get; set; }
        public string Url { get; set; } = null!;
        public ContentLink? ParentLink { get; set; }
        public DateTime? Changed { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? StartPublish { get; set; }
        public DateTime? Saved { get; set; }
    }
}
