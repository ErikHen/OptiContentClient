namespace OptiContentClient.Models.Properties
{
    /// <summary>
    /// Used for PropertyLongString and PropertyXhtmlString
    /// </summary>
    public class PropertyString : PropertyBase
    {
        public string? Value { get; set; }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }
    }
}
