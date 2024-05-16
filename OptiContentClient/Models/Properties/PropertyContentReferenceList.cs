
namespace OptiContentClient.Models.Properties
{
    public class PropertyContentReferenceList : PropertyBase
    {
        public ContentLink[]? Value { get; set; }
        public Content[]? ExpandedValue { get; set; }
    }

    public class PropertyContentReferenceList<T> : PropertyBase where T : Content
    {
        public ContentLink[]? Value { get; set; }
        public T[]? ExpandedValue { get; set; }
    }
}
