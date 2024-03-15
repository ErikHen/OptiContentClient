namespace OptiContentClient.Models.Properties
{
    public class PropertyList<T> : PropertyBase
    {
        public T[] Value { get; set; } = null!;
    }
}
