
namespace OptiContentClient.Models.Properties
{
    /// <summary>
    /// Used for IList&lt;BlockData>
    /// </summary>
    public class PropertyCollection<T> : PropertyBase
    {
        public T[] Value { get; set; } = null!;
    }
}
