
namespace OptiContentClient.Models.Properties
{
    /// <summary>
    /// Used for IList&lt;BlockData>
    /// </summary>
    public class PropertyCollection : PropertyBase
    {
        public Link[] Value { get; set; } = null!;
        public Content[] ExpandedValue { get; set; } = null!;
    }
}
