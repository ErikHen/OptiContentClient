
namespace OptiContentClient.Models.Properties
{
    public class PropertyLink : PropertyLinkBase
    {
        public Content? ExpandedValue { get; set; }
    }

    public class PropertyLink<T> : PropertyLinkBase where T : Content
    {
        public T? ExpandedValue { get; set; }
    }

    public abstract class PropertyLinkBase : PropertyBase
    {
        public Link? Value { get; set; }

        public override string ToString()
        {
            return Value?.Href ?? string.Empty;
        }
    }
}
