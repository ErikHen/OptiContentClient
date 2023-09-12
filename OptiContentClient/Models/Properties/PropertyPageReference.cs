
namespace OptiContentClient.Models.Properties
{
    public class PropertyPageReference : PropertyBase
    {
        public ContentLink Value { get; set; } = null!;
        public Content? ExpandedValue { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
