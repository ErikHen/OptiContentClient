namespace OptiContentClient.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CmsContentTypeAttribute : Attribute
    {
        public string Name { get; }

      //  public string? View { get; set; }

        public CmsContentTypeAttribute(string name) //, string? view = null)
        {
            Name = name;
       //     View = view;
        }
    }
}