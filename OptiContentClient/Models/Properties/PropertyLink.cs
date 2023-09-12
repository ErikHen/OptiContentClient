
namespace OptiContentClient.Models.Properties
{
    public class PropertyLink : PropertyBase
    {
        public Link Value { get; set; } = null!;
        public Content? ExpandedValue { get; set; }

        public override string ToString()
        {
            return Value.Href;
        }
    }
}
