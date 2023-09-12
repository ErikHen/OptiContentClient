
namespace OptiContent.Models.Properties
{
    public class PropertyMedia : PropertyBase
    {
        public ContentLink? Value { get; set; }
        public Media? ExpandedValue { get; set; }
        public string? Url => ExpandedValue?.Url ?? Value?.Url;

        public override string ToString()
        {
            return Url ?? string.Empty;
        }
    }
}
