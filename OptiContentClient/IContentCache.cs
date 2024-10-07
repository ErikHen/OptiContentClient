namespace OptiContentClient
{
    public interface IContentCache
    {
        Task Set(string key, ContentContainer contentContainer, TimeSpan expiresAfter);
        Task SetRefreshStarted(string key, ContentContainer contentContainer);
        Task<ContentContainer?> Get(string key);
    }
}
