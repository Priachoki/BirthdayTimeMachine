using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // Response from the Open-Meteo Historical Weather API
    // https://open-meteo.com/en/docs/historical-weather-api
    public class WeatherHistoryResponse
    {
        [JsonPropertyName("daily")]
        public DailyWeatherData? Daily { get; set; }
    }

    // The API returns each value as a list (one item per day)
    // values can be null if data is missing for that day
    public class DailyWeatherData
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("weather_code")]
        public List<int?> WeatherCode { get; set; } = new();

        [JsonPropertyName("temperature_2m_max")]
        public List<double?> MaxTemperature { get; set; } = new();

        [JsonPropertyName("temperature_2m_min")]
        public List<double?> MinTemperature { get; set; } = new();

        [JsonPropertyName("precipitation_sum")]
        public List<double?> Precipitation { get; set; } = new();

        [JsonPropertyName("wind_speed_10m_max")]
        public List<double?> MaxWindSpeed { get; set; } = new();
    }
}