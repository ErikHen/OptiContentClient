using OptiContentClient.Models;

namespace OptiContentClient
{
    public interface IContentCache
    {
        void Set(string key, ContentContainer contentContainer, TimeSpan expiresAfter);
        ContentContainer? Get(string key);

        //TODO: void Clear(string key);
    }
}
