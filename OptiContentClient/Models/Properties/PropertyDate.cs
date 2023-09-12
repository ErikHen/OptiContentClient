namespace OptiContentClient.Models.Properties
{
    public class PropertyDate : PropertyBase
    {
        public DateTime? Value { get; set; }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
