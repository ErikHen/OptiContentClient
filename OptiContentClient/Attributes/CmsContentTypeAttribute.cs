namespace OptiContentClient.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CmsContentTypeAttribute : Attribute
    {
        public string ContentTypeName { get; }

        public CmsContentTypeAttribute(string contentTypeName)
        {
            ContentTypeName = contentTypeName;
        }
    }
}