using System.Net;
using System.Text.Json;
using System.Web;
using OptiContentClient.JsonConverters;
using OptiContentClient.Models;

namespace OptiContentClient.Services
{
    public class ContentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ContentClientOptions _clientOptions;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IContentCache _contentCache;

        public ContentService(IHttpClientFactory httpClientFactory, ContentClientOptions clientOptions, IContentCache contentCache)
        {
            _contentCache = contentCache;
            _httpClientFactory = httpClientFactory;
            _clientOptions = clientOptions;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            _jsonOptions.Converters.Add(new ContentConverter());
        }

        public async Task<ContentContainer<T>> GetContentByPath<T>(string path, bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null) where T: Content
        {
            var contentContainer = await GetContentByPath(path, ignoreCache, overrideCacheSoftTtlSeconds);
            return CastToTyped<T>(contentContainer);
        }

        public async Task<ContentContainer> GetContentByPath(string path, bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null)
        {
            var isEditOrPreviewMode = false;
            var editModeQuery = string.Empty;
            var pathParts = path.Split('?');
            var purePath = pathParts[0]; //remove query parameters from path, because otherwise they will be part of the cache key
            if (pathParts.Length > 1)
            {
                var queryParameters = HttpUtility.ParseQueryString("?" + pathParts[1]);
                var editModeValue = queryParameters["epieditmode"];
                isEditOrPreviewMode = editModeValue != "";
                editModeQuery = isEditOrPreviewMode ? "&epieditmode=" + editModeValue : ""; //but keep edit mode parameter because it might be needed for the CMS request
            }

            return await GetContentFromCacheOrCms(purePath + $"?expand=*{editModeQuery}", string.Empty, false, ignoreCache || isEditOrPreviewMode, overrideCacheSoftTtlSeconds);
        }


        public async Task<ContentContainer<T>> GetChildren<T>(string contentIdentifier, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null) where T : Content
        {
            var contentContainer = await GetChildren(contentIdentifier, language, expand, select, additionalQuery, ignoreCache, overrideCacheSoftTtlSeconds);
            return CastToTyped<T>(contentContainer);
        }

        public async Task<ContentContainer> GetChildren(string contentIdentifier, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null)
        {
            var expandQuery = expand != "" ? $"expand={expand}&" : "";
            var selectQuery = select != "" ? $"select={select}&" : "";
            var fullPathAndQuery = $"/api/episerver/v3.0/content/{contentIdentifier}/children?{expandQuery}{selectQuery}{additionalQuery}";

            return await GetContentFromCacheOrCms(fullPathAndQuery, language, true, ignoreCache, overrideCacheSoftTtlSeconds);
        }


        public async Task<ContentContainer<T>> GetAncestors<T>(string contentIdentifier, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null) where T : Content
        {
            var contentContainer = await GetAncestors(contentIdentifier, language, expand, select, additionalQuery, ignoreCache, overrideCacheSoftTtlSeconds);
            return CastToTyped<T>(contentContainer);
        }

        public async Task<ContentContainer> GetAncestors(string contentIdentifier, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null)
        {
            var expandQuery = expand != "" ? $"expand={expand}&" : "";
            var selectQuery = select != "" ? $"select={select}&" : "";
            var fullPathAndQuery = $"/api/episerver/v3.0/content/{contentIdentifier}/ancestors?{expandQuery}{selectQuery}{additionalQuery}";

            return await GetContentFromCacheOrCms(fullPathAndQuery, language, true, ignoreCache, overrideCacheSoftTtlSeconds);
        }


        public async Task<ContentContainer<T>> GetContent<T>(string[] contentGuids, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null) where T : Content
        {
            var contentContainer = await GetContent(contentGuids, language, expand, select, additionalQuery, ignoreCache, overrideCacheSoftTtlSeconds);
            return CastToTyped<T>(contentContainer);
        }

        public async Task<ContentContainer> GetContent(string[] contentGuids, string language = "", string expand = "", string select = "", string additionalQuery = "", bool ignoreCache = false, int? overrideCacheSoftTtlSeconds = null)
        {
            var contentGuidsQuery = "guids=" + string.Join(",", contentGuids) + "&";
            var expandQuery = expand != "" ? $"expand={expand}&" : "";
            var selectQuery = select != "" ? $"select={select}&" : "";
            var fullPathAndQuery = $"/api/episerver/v3.0/content?{contentGuidsQuery}{expandQuery}{selectQuery}{additionalQuery}";

            return await GetContentFromCacheOrCms(fullPathAndQuery, language, true, ignoreCache, overrideCacheSoftTtlSeconds);
        }


        private static ContentContainer<T> CastToTyped<T>(ContentContainer container) where T : Content
        {
            var typedContentContainer = new ContentContainer<T>
            {
                Content = container.Content.Cast<T>().ToArray(),
                ExpiresAt = container.ExpiresAt,
                FetchStatus = container.FetchStatus,
                FetchedFromCmsAt = container.FetchedFromCmsAt,
                Message = container.Message
            };
            return typedContentContainer;
        }

        private async Task<ContentContainer> GetContentFromCacheOrCms(string pathAndQuery, string language, bool multipleItems, bool ignoreCache, int? overrideCacheSoftTtlSeconds)
        {
            ContentContainer? contentContainer;
            if (ignoreCache)
            {
                contentContainer = await GetContentFromCms(pathAndQuery, language, multipleItems, overrideCacheSoftTtlSeconds);
            }
            else
            {
                var cacheKey = GetCacheKey(pathAndQuery, language);
                contentContainer = await _contentCache.Get(cacheKey);
                if (contentContainer == null)
                {
                    //Cache is empty, fetch from CMS.
                    contentContainer = await GetContentFromCmsAndAddToCache(pathAndQuery, language, multipleItems, ignoreCache, overrideCacheSoftTtlSeconds);
                }
                else if (contentContainer.ShouldRefresh())
                {
                    //Cache is not empty, but content has expired.
                    //Set that refresh has started, so that other requests don't try to refresh the same content.
                    contentContainer.LastRefreshStartedAt = DateTime.UtcNow;
                    await _contentCache.Set(cacheKey, contentContainer, TimeSpan.FromSeconds(_clientOptions.CacheHardTtlSeconds));

                    //Fetch new content from CMS in background thread.
                    _ = Task.Run(() => GetContentFromCmsAndAddToCache(pathAndQuery, language, multipleItems, ignoreCache, overrideCacheSoftTtlSeconds));
                }
            }

            return contentContainer;
        }

        private async Task<ContentContainer> GetContentFromCmsAndAddToCache(string pathAndQuery, string language, bool multipleItems, bool ignoreCache, int? overrideCacheSoftTtlSeconds)
        {
            //If there are too many failed requests, don't try to fetch from CMS.
            if (FailCounterService.FailCount > _clientOptions.FailedFetchLimit)
            {
                return new ContentContainer { FetchStatus = HttpStatusCode.ServiceUnavailable, Message = "Too many failed requests to CMS. Waiting some time before trying again." };
            }

            var contentFromCms = await GetContentFromCms(pathAndQuery, language, multipleItems, overrideCacheSoftTtlSeconds);
            if (contentFromCms.FetchStatus == HttpStatusCode.OK)
            {
                if (!ignoreCache)
                {
                    await _contentCache.Set(GetCacheKey(pathAndQuery, language), contentFromCms, TimeSpan.FromSeconds(_clientOptions.CacheHardTtlSeconds));
                }
            }
            else if (contentFromCms.FetchStatus == HttpStatusCode.NotFound)
            {
                if (!ignoreCache)
                {
                    //NotFound is also cached to prevent multiple requests for the same non-existing content to reach the CMS.
                    //Content could also have been deleted/unpublished in CMS, and cache needs to be updated with this info.
                    await _contentCache.Set(GetCacheKey(pathAndQuery, language), contentFromCms, TimeSpan.FromSeconds(_clientOptions.CacheHardTtlSeconds));
                }

                //NotFound is also added to fail counter. This will prevent massive amount of requests to different non-existing content to overload the CMS.
                AddFailCount(contentFromCms.FetchStatus);
            }
            else
            {
                AddFailCount(contentFromCms.FetchStatus);
            }

            return contentFromCms;
        }

        private static string GetCacheKey(string pathAndQuery, string language)
        {
            return pathAndQuery + "_" + language;
        }

        private async Task<ContentContainer> GetContentFromCms(string pathAndQuery, string language, bool multipleItems, int? overrideCacheSoftTtlSeconds)
        {
            var contentContainer = new ContentContainer();
            var url = $"{_clientOptions.BaseUrl}{pathAndQuery}";
            var httpClient = GetHttpClient();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(language))
                {
                    request.Headers.Add("Accept-Language", language);
                }
                var response = await httpClient.SendAsync(request);
                    
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (multipleItems)
                    {
                        var contentItems = JsonSerializer.Deserialize<Content[]>(responseString, _jsonOptions);
                        contentContainer.Content = contentItems ?? Array.Empty<Content>();
                    }
                    else
                    {
                        var content = JsonSerializer.Deserialize<Content>(responseString, _jsonOptions);
                        if (content != null)
                        {
                            contentContainer.Content = new[] { content };
                        }
                    }
                }

                contentContainer.FetchedFromCmsAt = DateTime.UtcNow;
                contentContainer.ExpiresAt = DateTime.UtcNow.AddSeconds(overrideCacheSoftTtlSeconds ?? _clientOptions.CacheSoftTtlSeconds);
                contentContainer.FetchStatus = response.StatusCode;
            }
            catch (TaskCanceledException)
            {
                //fetch-timeout error https://www.tabsoverspaces.com/233134-taskcanceledexception-on-timeout-on-httpclient
                contentContainer.FetchStatus = HttpStatusCode.RequestTimeout;
            }
            catch (Exception e)
            {
                contentContainer.FetchStatus = HttpStatusCode.InternalServerError;
                contentContainer.Message = e.Message;
            }

            return contentContainer;
        }

        private void AddFailCount(HttpStatusCode fetchStatus)
        {
            if (fetchStatus is HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout) //check for more status codes?
            {
                FailCounterService.AddFail();
            }
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(ContentService));
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.Timeout = TimeSpan.FromMilliseconds(_clientOptions.TimeoutMilliseconds);
            return httpClient;
        }
    }
}
