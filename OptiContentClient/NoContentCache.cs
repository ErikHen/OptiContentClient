namespace OptiContentClient
{
    internal class NoContentCache : IContentCache
    {
        public ContentContainer? Get(string key)
        {
            return null;
        }

        public void Set(string key, ContentContainer contentContainer, TimeSpan expiresAfter)
        {
        }
    }
}
