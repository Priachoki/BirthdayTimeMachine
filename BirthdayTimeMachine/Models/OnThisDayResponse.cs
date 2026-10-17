using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // Response from Wikipedia's "On this day" feed
    // https://en.wikipedia.org/api/rest_v1/#/Feed/onThisDay
    public class OnThisDayResponse
    {
        [JsonPropertyName("events")]
        public List<WikiEvent>? Events { get; set; }

        [JsonPropertyName("births")]
        public List<WikiEvent>? Births { get; set; }
    }

    public class WikiEvent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("pages")]
        public List<WikiPage>? Pages { get; set; }
    }

    public class WikiPage
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // not every article has a picture
        [JsonPropertyName("thumbnail")]
        public WikiImage? Thumbnail { get; set; }

        [JsonPropertyName("content_urls")]
        public WikiContentUrls? ContentUrls { get; set; }
    }

    public class WikiImage
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }

    public class WikiContentUrls
    {
        [JsonPropertyName("desktop")]
        public WikiPageLink? Desktop { get; set; }
    }

    public class WikiPageLink
    {
        [JsonPropertyName("page")]
        public string Page { get; set; } = string.Empty;
    }
}