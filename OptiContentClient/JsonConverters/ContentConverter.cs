using System.Text.Json;
using System.Text.Json.Serialization;
using OptiContentClient.Models;
#pragma warning disable CA1869

namespace OptiContentClient.JsonConverters
{
    public class ContentConverter : JsonConverter<Content>
    {
        public override Content Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var contentTypeMapping = TryGetContentTypeMapping(doc.RootElement);

            if (contentTypeMapping != null)
            {
                var rawText = doc.RootElement.GetRawText();
                try
                {
                    if (!(JsonSerializer.Deserialize(rawText, contentTypeMapping.ModelType, options) is Content item))
                    {
                        throw new Exception($"Unable to deserialize {rawText} to {contentTypeMapping.ModelType}");
                    }

                    return item;
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Unable to deserialize ({ex.Message}): {rawText}", ex);
                }
            }

            //no mapping found, return a generic content.
            return new Content
            {
                Name = doc.RootElement.GetProperty("name").GetString() ?? "",
                ContentType = GetContentTypes(doc.RootElement),
            };
        }

        public override void Write(Utf8JsonWriter writer, Content value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            if (type == typeof(Content))
            {
                //Can't use options that include this converter, because it will mean an infinite loop.
                //So, create a new options object without this converter, and serialize. Probably not a very performant solution, but this is a very rare edge case.
                JsonSerializer.Serialize(writer, value, type, new JsonSerializerOptions{ PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive, PropertyNamingPolicy = options.PropertyNamingPolicy});
            }
            else
            {
                JsonSerializer.Serialize(writer, value, type, options);
            }
        }

        /// <summary>
        /// Find the content type name and find the model type for that content type.
        /// </summary>
        private static ContentTypeMappings.Mapping? TryGetContentTypeMapping(JsonElement element)
        {
            //if there is a contentType property, get the mapping from one of the content type names.
            if (element.TryGetProperty("contentType", out var contentTypeElement))
            {
                var contentTypes = contentTypeElement.EnumerateArray();
                //loop types backwards to find the least generic type
                foreach (var item in contentTypes.Reverse())
                {
                    if (ContentTypeMappings.Mappings.TryGetValue(item.GetString() ?? "", out var mapping))
                    {
                        return mapping;
                    }
                };
            }

            //if this content is in a IList (PropertyCollection) the content type name is instead in the "propertyItemType" property (thank´s Optimizely for this inconsistency...).
            if (element.TryGetProperty("propertyItemType", out var propertyItemTypeElement))
            {
                if (ContentTypeMappings.Mappings.TryGetValue(propertyItemTypeElement.GetString() ?? "", out var mapping))
                {
                    return mapping;
                }
            }

            return null;
        }

        private static string[] GetContentTypes(JsonElement element)
        {
            var contentTypeElement = element.GetProperty("contentType");
            return contentTypeElement.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
        }
        
    }
}
