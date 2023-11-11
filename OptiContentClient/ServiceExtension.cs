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
        }
    }
}
