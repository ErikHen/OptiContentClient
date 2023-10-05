
namespace OptiContentClient.Models
{
    public class Content
    {
        public string? Name { get; set; }
        public ContentLink ContentLink { get; set; } = null!;
        public string[] ContentType { get; set; } = null!;
        public string? RouteSegment { get; set; }
        public ContentLanguage? Language { get; set; }
        public ContentLanguage[]? ExistingLanguages { get; set; }
        public string Url { get; set; } = null!;
    }
}
