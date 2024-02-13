
namespace OptiContentClient.Models
{
    public class Content
    {
        public string Name { get; set; } = null!;
        public ContentLink ContentLink { get; set; }
        public string[] ContentType { get; set; }
        public string RouteSegment { get; set; } = null!;
        public ContentLanguage Language { get; set; }
        public ContentLanguage[]? ExistingLanguages { get; set; }
        public string Url { get; set; } = null!;
        public ContentLink? ParentLink { get; set; }
        public DateTime? Changed { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? StartPublish { get; set; }
        public DateTime? Saved { get; set; }

        public Content()
        {
            ContentLink = new ContentLink();
            Language = new ContentLanguage();
            ContentType = Array.Empty<string>();
        }
    }

}
