using System.Net;
using OptiContentClient.Models;

namespace OptiContentClient
{
    public abstract class ContentContainerBase
    {
        public HttpStatusCode FetchStatus { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// UTC 
        /// </summary>
        public DateTime FetchedFromCmsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastRefreshStartedAt { get; set; }

        public bool ShouldRefresh()
        {
            if (ExpiresAt < DateTime.UtcNow)
            {
                //data in cache is expired
                if (!LastRefreshStartedAt.HasValue || LastRefreshStartedAt < DateTime.UtcNow.AddSeconds(-30))
                {
                    //no refresh in progress, or it started more than 30 seconds ago
                    return true;
                }
            }

            return false;
        }
    }

    public class ContentContainer : ContentContainerBase
    {
        public Content[] Content { get; set; } = Array.Empty<Content>();
    }

    public class ContentContainer<T> : ContentContainerBase where T : Content
    {
        public T[] Content { get; set; } = Array.Empty<T>();
    }
}
