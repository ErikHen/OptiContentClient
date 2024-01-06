
namespace OptiContentClient.Models.Properties
{
    public class PropertyPageReference : PropertyPageReferenceBase
    {
        public Content? ExpandedValue { get; set; }
    }

    public class PropertyPageReference<T> : PropertyPageReferenceBase where T : Content
    {
        public T? ExpandedValue { get; set; }
    }

    public abstract class PropertyPageReferenceBase : PropertyBase
    {
        public ContentLink Value { get; set; } = null!;
        public virtual string? Url => Value.Url;

        public override string ToString()
        {
            return Url ?? string.Empty;
        }
    }
}
