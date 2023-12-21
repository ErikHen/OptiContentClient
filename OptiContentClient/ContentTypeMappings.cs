using OptiContentClient.Attributes;
using OptiContentClient.Models;

namespace OptiContentClient
{
    public static class ContentTypeMappings
    {
        private static IDictionary<string, Mapping>? _mappingsCache; // static, because it should be "valid" for as long as the app is running

        public static IDictionary<string, Mapping> Mappings
        {
            get
            {
                if (_mappingsCache == null)
                {
                    var components = from a in AppDomain.CurrentDomain.GetAssemblies()
                        from t in a.GetTypes()
                        let attributes = t.GetCustomAttributes(typeof(CmsContentTypeAttribute), true)
                        where attributes != null && attributes.Length > 0
                        select new {Type = t, Attribute = attributes.Cast<CmsContentTypeAttribute>().First()};

                    var mappingsCache = new Dictionary<string, Mapping>();
                    foreach (var c in components)
                    {
                        if (mappingsCache.ContainsKey(c.Attribute.Name))
                        {
                            continue;
                        }
                        
                        mappingsCache[c.Attribute.Name] = new Mapping
                        {
                            Type = c.Type,
                            ComponentName = c.Attribute.Name,
                        };
                    }

                    // If "Media"-type is not defined, add it
                    mappingsCache.TryAdd("Media", new Mapping
                    {
                        Type = typeof(Media),
                        ComponentName = "",
                    });

                    _mappingsCache = mappingsCache; // this makes sure we don't have concurrent operations on the dictionary while it's filling
                    return mappingsCache;
                }

                return _mappingsCache;
            }
        }

        public class Mapping
        {
            public string ComponentName { get; set; } = "";
            public Type Type { get; set; } = typeof(object);
        }
    }
}