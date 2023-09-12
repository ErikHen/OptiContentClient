
namespace OptiContentClient.Models.Properties
{
    public class PropertyContentReference : PropertyBase
    {
        public ContentLink? Value { get; set; }
        public Content? ExpandedValue { get; set; }
        public string? Url => ExpandedValue?.Url ?? Value?.Url;

        public override string ToString()
        {
            return Url ?? string.Empty;
        }
    }
}
