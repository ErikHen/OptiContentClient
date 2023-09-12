using OptiContent.Models.Properties;

namespace OptiContent.Models
{
    public class Media : Content
    {
        //will thumbnail ever be needed?  public PropertyBlob? Thumbnail { get; set; } 
        public PropertyString? MimeType { get; set; }
    }
}
