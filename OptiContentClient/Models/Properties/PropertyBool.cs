
namespace OptiContentClient.Models.Properties
{
    public class PropertyBool : PropertyBase
    {
        public bool? Value { get; set; }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
