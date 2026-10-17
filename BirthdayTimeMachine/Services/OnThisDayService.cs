using System.Net.Http.Json;
using System.Text.Json;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;

namespace BirthdayTimeMachine.Services
{
    // Calls Wikipedia's "On this day" feed
    public class OnThisDayService : IOnThisDayService
    {
        private readonly HttpClient _httpClient;

        public OnThisDayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OnThisDayResult?> GetOnThisDayAsync(DateOnly date, int maxItems = 5)
        {
            if (maxItems <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxItems), "maxItems must be at least 1.");
            }

            // month and day must be two digits, e.g. 03/12
            string url = $"api/rest_v1/feed/onthisday/all/{date.Month:D2}/{date.Day:D2}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Wikipedia returned {(int)response.StatusCode}.");
            }

            OnThisDayResponse? data;
            try
            {
                data = await response.Content.ReadFromJsonAsync<OnThisDayResponse>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Wikipedia sent back data we couldn't read.");
            }

            if (data == null)
            {
                return null;
            }

            List<HistoryEvent> events = ConvertEvents(data.Events);
            List<HistoryEvent> births = ConvertEvents(data.Births);

            return new OnThisDayResult
            {
                // closest to the birth year first
                Events = events
                    .OrderBy(e => Math.Abs((e.Year ?? 0) - date.Year))
                    .Take(maxItems)
                    .ToList(),

                Births = births
                    .OrderBy(e => Math.Abs((e.Year ?? 0) - date.Year))
                    .Take(maxItems)
                    .ToList(),

                SameYearEvents = events
                    .Where(e => e.Year == date.Year)
                    .ToList()
            };
        }

        // turns the nested Wikipedia data into simple HistoryEvent objects
        private static List<HistoryEvent> ConvertEvents(List<WikiEvent>? wikiEvents)
        {
            if (wikiEvents == null)
            {
                return new List<HistoryEvent>();
            }

            return wikiEvents
                .Where(e => !string.IsNullOrWhiteSpace(e.Text))
                .Select(e =>
                {
                    WikiPage? firstPage = e.Pages?.FirstOrDefault();

                    return new HistoryEvent
                    {
                        Year = e.Year,
                        Text = e.Text,
                        ImageUrl = firstPage?.Thumbnail?.Source,
                        ArticleUrl = firstPage?.ContentUrls?.Desktop?.Page
                    };
                })
                .ToList();
        }
    }
}