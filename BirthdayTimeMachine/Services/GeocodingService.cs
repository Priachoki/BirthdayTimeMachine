using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;
using System.Text.Json;

namespace BirthdayTimeMachine.Services
{
    // Calls the Open-Meteo Geocoding API
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;

        // HttpClient is passed in by dependency injection (set up in Program.cs)
        public GeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeocodingResult?> GetLocationAsync(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                throw new ArgumentException("Please enter a city name.", nameof(cityName));
            }

            // EscapeDataString handles spaces and special characters, e.g. "New York"
            string url = $"v1/search?name={Uri.EscapeDataString(cityName.Trim())}&count=1&language=en&format=json";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Geocoding service returned {(int)response.StatusCode}.");
            }

            try
            {
                GeocodingResponse? data = await response.Content.ReadFromJsonAsync<GeocodingResponse>();

                // no results = city not found
                return data?.Results?.FirstOrDefault();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The geocoding service sent back data we couldn't read.");
            }
        }
    }
}