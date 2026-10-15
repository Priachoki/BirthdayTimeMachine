using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // Response from the Open-Meteo Geocoding API
    // https://open-meteo.com/en/docs/geocoding-api
    public class GeocodingResponse
    {
        // null if the city isn't found
        [JsonPropertyName("results")]
        public List<GeocodingResult>? Results { get; set; }
    }

    // One place from the results list
    public class GeocodingResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        // e.g. "IE" - TODO: use for holidays and country info later
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        // region/county, not every place has one
        [JsonPropertyName("admin1")]
        public string? Region { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        // e.g. "Dublin, Leinster, Ireland"
        // TODO: unit test with and without a region
        public string DisplayName =>
            string.IsNullOrWhiteSpace(Region)
                ? $"{Name}, {Country}"
                : $"{Name}, {Region}, {Country}";
    }
}