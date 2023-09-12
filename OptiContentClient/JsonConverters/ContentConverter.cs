using System.Text.Json;
using System.Text.Json.Serialization;
using OptiContentClient.Models;

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
                    if (!(JsonSerializer.Deserialize(rawText, contentTypeMapping.Type, options) is Content item))
                    {
                        throw new Exception($"Unable to deserialize {rawText} to {contentTypeMapping.Type}");
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

        private static ContentTypeMappings.Mapping? TryGetContentTypeMapping(JsonElement element)
        {
            var contentTypes = element.GetProperty("contentType").EnumerateArray();
            //loop types backwards to find the least generic type
            foreach (var item in contentTypes.Reverse())
            {
                if (ContentTypeMappings.Mappings.TryGetValue(item.GetString() ?? "", out var mapping))
                {
                    return mapping;
                }
            };

            return null;
        }

        private static string[] GetContentTypes(JsonElement element)
        {
            var contentTypeElement = element.GetProperty("contentType");
            return contentTypeElement.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
        }

        public override void Write(Utf8JsonWriter writer, Content value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
