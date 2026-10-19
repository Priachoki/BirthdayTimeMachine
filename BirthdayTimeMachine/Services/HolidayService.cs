using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace BirthdayTimeMachine.Services
{
    // Calls Nager.Date to get public holidays for a country and year
    public class HolidayService : IHolidayService
    {
        private readonly HttpClient _httpClient;

        public HolidayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HolidayResult?> GetHolidayInfoAsync(DateOnly date, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return null;
            }

            string code = Uri.EscapeDataString(countryCode.Trim().ToUpperInvariant());
            string url = $"api/v3/PublicHolidays/{date.Year}/{code}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // country not supported
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"The holiday service returned {(int)response.StatusCode}.");
            }

            string body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            List<PublicHoliday>? holidays;
            try
            {
                holidays = JsonSerializer.Deserialize<List<PublicHoliday>>(body);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The holiday service sent back data we couldn't read.");
            }

            return holidays == null ? null : FindClosest(holidays, date);
        }

        // finds the holiday on the birthday, or the nearest one that year
        public static HolidayResult? FindClosest(IEnumerable<PublicHoliday> holidays, DateOnly birthday)
        {
            List<PublicHoliday> withDates = holidays.Where(h => h.Date.HasValue).ToList();

            if (withDates.Count == 0)
            {
                return null;
            }

            PublicHoliday closest = withDates
                .OrderBy(h => Math.Abs(h.Date!.Value.DayNumber - birthday.DayNumber))
                .First();

            int difference = closest.Date!.Value.DayNumber - birthday.DayNumber;

            return new HolidayResult
            {
                Holiday = closest,
                IsOnBirthday = difference == 0,
                DaysFromBirthday = difference
            };
        }
    }
}