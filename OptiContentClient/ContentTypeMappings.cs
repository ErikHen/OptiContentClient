using OptiContentClient.Attributes;
using OptiContentClient.Models;

namespace OptiContentClient
{
    public static class ContentTypeMappings
    {
        private static IDictionary<string, Mapping>? _mappingsCache; // make static, because it should be "valid" for as long as the app is running

        public static IDictionary<string, Mapping> Mappings
        {
            get
            {
                if (_mappingsCache == null)
                {
                    // find all (model) types with the CmsContentTypeAttribute. Add the type and the attribute to a collection.
                    var typeAndAttributeCollection = from a in AppDomain.CurrentDomain.GetAssemblies()
                        from t in a.GetTypes()
                        let attributes = t.GetCustomAttributes(typeof(CmsContentTypeAttribute), true)
                        where attributes != null && attributes.Length > 0
                        select new {Type = t, Attribute = attributes.Cast<CmsContentTypeAttribute>().First()};

                    // add all "content type name" -> "model type" mappings to a dictionary
                    var mappingsCache = new Dictionary<string, Mapping>();
                    foreach (var ta in typeAndAttributeCollection)
                    {
                        mappingsCache.TryAdd(ta.Attribute.ContentTypeName, new Mapping
                        {
                            ModelType = ta.Type,
                            ContentTypeName = ta.Attribute.ContentTypeName,
                        });
                    }

                    // If "Media"-type is not defined, add it
                    mappingsCache.TryAdd("Media", new Mapping
                    {
                        ModelType = typeof(Media),
                        ContentTypeName = "",
                    });

                    _mappingsCache = mappingsCache; // this makes sure we don't have concurrent operations on the dictionary while it's filling
                    return mappingsCache;
                }

                return _mappingsCache;
            }
        }

        public class Mapping
        {
            public string ContentTypeName { get; set; } = "";
            public Type ModelType { get; set; } = typeof(object);
        }
    }
}