using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // Response from TMDB's "discover movie" endpoint
    // https://developer.themoviedb.org/reference/discover-movie
    public class TmdbDiscoverResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMovie>? Results { get; set; }
    }

    public class TmdbMovie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // e.g. "2005-03-11", can be empty
        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        // e.g. "/abc123.jpg" - not every film has one
        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }
}