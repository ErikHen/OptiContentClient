namespace OptiContentClient.Models
{
    public class Link
    {
        public string Href { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string? Target { get; set; }
        public string? Title { get; set; }
        public ContentLink? ContentLink { get; set; }

        public override string ToString()
        {
            return Href;
        }
    }
}
