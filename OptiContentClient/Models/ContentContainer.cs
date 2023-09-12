using System.Net;

namespace OptiContentClient.Models
{
    public class ContentContainer
    {
        public HttpStatusCode FetchStatus { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// UTC 
        /// </summary>
        public DateTime FetchedFromCmsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Content[] Content { get; set; } = Array.Empty<Content>();

    }

}
