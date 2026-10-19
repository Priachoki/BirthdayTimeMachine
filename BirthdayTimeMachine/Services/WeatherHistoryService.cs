using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;
using System.Globalization;
using System.Text.Json;

namespace BirthdayTimeMachine.Services
{
    // Calls the Open-Meteo Historical Weather API
    public class WeatherHistoryService : IWeatherHistoryService
    {
        // the archive only goes back to 1940
        public static readonly DateOnly EarliestDate = new DateOnly(1940, 1, 1);

        private readonly HttpClient _httpClient;

        public WeatherHistoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherDay?> GetWeatherForDateAsync(double latitude, double longitude, DateOnly date)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (date < EarliestDate || date >= today)
            {
                throw new ArgumentOutOfRangeException(nameof(date),
                    $"Weather history is only available from {EarliestDate:dd MMM yyyy} up to yesterday.");
            }

            // InvariantCulture makes sure decimals use a dot (53.35, not 53,35)
            string lat = latitude.ToString(CultureInfo.InvariantCulture);
            string lon = longitude.ToString(CultureInfo.InvariantCulture);
            string day = date.ToString("yyyy-MM-dd");

            string url = $"v1/archive?latitude={lat}&longitude={lon}" +
                         $"&start_date={day}&end_date={day}" +
                         "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max" +
                         "&timezone=auto";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Weather service returned {(int)response.StatusCode}.");
            }

            WeatherHistoryResponse? data;
            try
            {
                data = await response.Content.ReadFromJsonAsync<WeatherHistoryResponse>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The weather service sent back data we couldn't read.");
            }

            DailyWeatherData? daily = data?.Daily;
            if (daily == null || daily.Time.Count == 0)
            {
                return null;
            }

            // we only asked for one day, so everything is at index 0
            return new WeatherDay
            {
                Date = date,
                WeatherCode = daily.WeatherCode.FirstOrDefault(),
                MaxTemperature = daily.MaxTemperature.FirstOrDefault(),
                MinTemperature = daily.MinTemperature.FirstOrDefault(),
                Precipitation = daily.Precipitation.FirstOrDefault(),
                MaxWindSpeed = daily.MaxWindSpeed.FirstOrDefault()
            };
        }
    }
}