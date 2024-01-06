
namespace OptiContentClient.Models.Properties
{
    public class PropertyContentReference : PropertyContentReferenceBase
    {
        public Content? ExpandedValue { get; set; }
        public override string? Url => ExpandedValue?.Url ?? Value?.Url;
    }

    public class PropertyContentReference<T> : PropertyContentReferenceBase where T : Content
    {
        public T? ExpandedValue { get; set; }
        public override string? Url => ExpandedValue?.Url ?? Value?.Url;
    }

    public abstract class PropertyContentReferenceBase : PropertyBase
    {
        public ContentLink? Value { get; set; }
        public virtual string? Url => Value?.Url;

        public override string ToString()
        {
            return Url ?? string.Empty;
        }
    }
}
