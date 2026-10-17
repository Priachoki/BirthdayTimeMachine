namespace BirthdayTimeMachine.Models
{
    // A film released around the user's birthday
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public DateOnly? ReleaseDate { get; set; }
        public string? PosterUrl { get; set; }
        public double Rating { get; set; }
        public string TmdbUrl { get; set; } = string.Empty;

        public string RatingText => Rating.ToString("0.0");
    }
}