
using System.Linq;

namespace OptiContentClient.Models.Properties
{
    public class PropertyContentReferenceList : PropertyBase
    {
        public ContentLink[]? Value { get; set; }
        public Content[]? ExpandedValue { get; set; }
    }
}
