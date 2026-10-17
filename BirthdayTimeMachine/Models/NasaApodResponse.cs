using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // Response from NASA's APOD endpoint on science.nasa.gov
    // e.g. https://science.nasa.gov/wp-json/wp/v2/apod-basic/050312
    // (the old api.nasa.gov endpoint started returning the NASA logo in Oct 2026)
    public class NasaApodResponse
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // "image" or "video"
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; } = string.Empty;

        // contains some HTML tags
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        // link to NASA's article page
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        // the actual picture
        [JsonPropertyName("hdurl")]
        public string? HdUrl { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }

        [JsonPropertyName("credit")]
        public string? Credit { get; set; }
    }
}