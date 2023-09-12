
namespace OptiContentClient.Models.Properties
{
    /// <summary>
    /// Use this for any number property (int or double).
    /// </summary>
    public class PropertyNumber : PropertyBase
    {
        public double? Value { get; set; }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
