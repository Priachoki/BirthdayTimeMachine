namespace BirthdayTimeMachine.Models
{
    // NASA's picture of the day, simplified for the app
    public class SpacePicture
    {
        public DateOnly Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? Copyright { get; set; }

        public bool IsVideo => !string.IsNullOrWhiteSpace(VideoUrl);
    }
}