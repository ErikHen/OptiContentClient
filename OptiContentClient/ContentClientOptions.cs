namespace OptiContentClient
{
    public class ContentClientOptions
    {
        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// Content will be considered expired after this, and new content will be fetched from CMS.
        /// </summary>
        public int CacheSoftTtlSeconds { get; set; } = 60 * 2;

        /// <summary>
        /// How long the content will be stored in the cache (expired or not).
        /// </summary>
        public int CacheHardTtlSeconds { get; set; } = 60 * 60 * 24 * 2;

        /// <summary>
        /// Number of milliseconds to wait when fetching content from CMS.
        /// </summary>
        public int TimeoutMilliseconds { get; set; } = 3000;

        /// <summary>
        /// Content service will not try to make any more requests to the CMS if there are repeating failures during a short period.
        /// </summary>
        public int FailedFetchLimit { get; set; } = 100;
    }
}
