
namespace OptiContentClient.Models.Properties
{
    public class PropertyLinkCollection : PropertyBase
    {
        public Link[] Value { get; set; } = null!;
        public Content[] ExpandedValue { get; set; } = null!;
    }
}
