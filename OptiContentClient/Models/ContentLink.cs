namespace OptiContentClient.Models
{
    public class ContentLink
    {
        public int Id { get; set; }
        public string GuidValue { get; set; } = null!;
        public string? Url { get; set; }

        public override string ToString()
        {
            return Url ?? string.Empty;
        }
    }
}
