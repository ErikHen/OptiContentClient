
using OptiContentClient.Models.Properties;

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

        /// <summary>
        /// MimeType is only available for media content.
        /// </summary>
        public PropertyString? MimeType { get; set; }
    }
}
