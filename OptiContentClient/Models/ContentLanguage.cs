namespace OptiContentClient.Models
{
    public class ContentLanguage
    {
        public string Name { get; set; } = null!;
        public string Link { get; set; } = null!;

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
