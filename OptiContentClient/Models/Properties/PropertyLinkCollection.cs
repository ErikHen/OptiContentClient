
namespace OptiContentClient.Models.Properties
{
    public class PropertyLinkCollection : PropertyBase
    {
        public Link[] Value { get; set; } = null!;
        public Content[] ExpandedValue { get; set; } = null!;
    }

    public class PropertyLinkCollection<T> : PropertyBase where T : Content
    {
        public Link[] Value { get; set; } = null!;
        public T[] ExpandedValue { get; set; } = null!;
    }
}
