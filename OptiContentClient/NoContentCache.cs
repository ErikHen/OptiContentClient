namespace OptiContentClient
{
    internal class NoContentCache : IContentCache
    {
        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ContentContainer?> Get(string key)
        {
            return null;
        }

        public async Task Set(string key, ContentContainer contentContainer, TimeSpan expiresAfter)
        {
        }
        #pragma warning restore CS1998

    }
}
