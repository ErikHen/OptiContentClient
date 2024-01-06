namespace OptiContentClient.Models.Properties
{
    public class PropertyContentArea : PropertyBase
    {
        public Content[]? ExpandedValue { get; set; }
        public ContentAreaValueItem[]? Value { get; set; }
    }

    /// <summary>
    /// Get strongly typed content from content area.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyContentArea<T> : PropertyBase where T : Content
    {
        public T[]? ExpandedValue { get; set; }
        public ContentAreaValueItem[]? Value { get; set; }
    }
}
