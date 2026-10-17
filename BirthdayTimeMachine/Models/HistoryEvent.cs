namespace BirthdayTimeMachine.Models
{
    // One event or famous birthday, simplified for the app
    public class HistoryEvent
    {
        public int? Year { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ArticleUrl { get; set; }

        public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    }
}