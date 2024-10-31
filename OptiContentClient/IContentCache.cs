namespace OptiContentClient
{
    public interface IContentCache
    {
        Task Set(string key, ContentContainer contentContainer, TimeSpan expiresAfter);
        Task<ContentContainer?> Get(string key);
    }
}
