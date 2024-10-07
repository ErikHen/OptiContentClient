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
                var cacheKey = pathAndQuery + "_" + language;
                contentContainer = await GetContentFromCache(cacheKey);
                if (contentContainer == null)
                {
                    //Cache is empty, fetch from CMS (but not if there are too many failed requests).
                    contentContainer = await GetContentFromCmsIfNotBackedOff(pathAndQuery, language, multipleItems, overrideCacheSoftTtlSeconds);
                    if (contentContainer.FetchStatus == HttpStatusCode.OK && !ignoreCache)
                    {
                        //TODO: should 404 be cached? Would improve performance if the same 404-url was requested repeatedly.
                        await AddContentToCache(cacheKey, contentContainer);
                    }
                    else
                    {
                        AddFailCount(contentContainer.FetchStatus);
                    }
                }
                else if (contentContainer.ExpiresAt < DateTime.UtcNow)
                {
                    //Cache is not empty, but content has expired. Fetch new content from CMS (but not if there are too many failed requests). 
                    var contentFromCms = await GetContentFromCmsIfNotBackedOff(pathAndQuery, language, multipleItems, overrideCacheSoftTtlSeconds);
                    if (contentFromCms.FetchStatus == HttpStatusCode.OK)
                    {
                        //Fetch was successful, so return new content.
                        contentContainer = contentFromCms;
                        if (!ignoreCache)
                        {
                            await AddContentToCache(cacheKey, contentContainer);
                        }
                    }
                    else
                    {
                        AddFailCount(contentContainer.FetchStatus);
                        contentContainer.FetchStatus = contentFromCms.FetchStatus; //update fetch status so it's possible to see why new fetch failed.
                        contentContainer.Message = contentFromCms.Message;
                    }
                }
            }

            return contentContainer;
        }

        private async Task<ContentContainer?> GetContentFromCache(string key)
        {
            var contentContainer = await _contentCache.Get(key);
            return contentContainer;
        }

        private async Task AddContentToCache(string key, ContentContainer contentContainer)
        {
            await _contentCache.Set(key, contentContainer, TimeSpan.FromSeconds(_clientOptions.CacheHardTtlSeconds));
        }

        private async Task<ContentContainer> GetContentFromCmsIfNotBackedOff(string pathAndQuery, string language, bool multipleItems, int? overrideCacheSoftTtlSeconds)
        {
            if (FailCounterService.FailCount > _clientOptions.FailedFetchLimit)
            {
                return new ContentContainer { FetchStatus = HttpStatusCode.ServiceUnavailable, Message = "Too many failed requests to CMS. Waiting some time before trying again." };
            }

            return await GetContentFromCms(pathAndQuery, language, multipleItems, overrideCacheSoftTtlSeconds);
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
                    
                    contentContainer.FetchedFromCmsAt = DateTime.UtcNow;
                    contentContainer.ExpiresAt = DateTime.UtcNow.AddSeconds(overrideCacheSoftTtlSeconds ?? _clientOptions.CacheSoftTtlSeconds);
                }
                
                contentContainer.FetchStatus = response.StatusCode;
            }
            catch (TaskCanceledException)
            {
                //fetch timeout error https://www.tabsoverspaces.com/233134-taskcanceledexception-on-timeout-on-httpclient
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
