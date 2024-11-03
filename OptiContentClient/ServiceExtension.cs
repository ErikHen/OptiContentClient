using Microsoft.Extensions.DependencyInjection;
using OptiContentClient.Services;

namespace OptiContentClient
{
    public static class ServiceExtension
    {
        public static void AddOptiContentClient(this IServiceCollection services, ContentClientOptions clientOptions)
        {
            if (services.All(x => x.ServiceType != typeof(IContentCache)))
            {
                services.AddSingleton<IContentCache, NoContentCache>();
            }
            services.AddHttpClient<ContentService>();
            services.AddSingleton(s => new ContentService(s.GetService<IHttpClientFactory>()!, clientOptions, s.GetService<IContentCache>()!));

            //This will fill the mappings cache add startup, instead of at first request.
            var mappings = ContentTypeMappings.Mappings;
            //This means a slight delay at startup, but in a scale-out scenario it's better to have it filled at startup, before a massive amount of requests hits the server
        }
    }
}
