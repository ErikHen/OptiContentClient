namespace OptiContentClient.Models.Properties
{
    /// <summary>
    /// Used for string, LongString, and XhtmlString
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
