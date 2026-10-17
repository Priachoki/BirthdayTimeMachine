using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;

namespace BirthdayTimeMachine.Services
{
    // Gets NASA's Astronomy Picture of the Day (APOD)
    // Uses science.nasa.gov because api.nasa.gov returns the NASA logo since Oct 2026
    // (see https://github.com/nasa/apod-api/issues/184)
    public class NasaApodService : INasaApodService
    {
        // APOD started on this date
        public static readonly DateOnly FirstApodDate = new DateOnly(1995, 6, 16);

        private readonly HttpClient _httpClient;

        public NasaApodService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static bool IsDateAvailable(DateOnly date)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            return date >= FirstApodDate && date < today;
        }

        // link to NASA's own page for that day, e.g. https://apod.nasa.gov/apod/ap050312.html
        public static string GetOfficialPageUrl(DateOnly date)
        {
            return $"https://apod.nasa.gov/apod/ap{date:yyMMdd}.html";
        }

        // still check for the generic logo in case the new endpoint has the same problem
        public static bool IsPlaceholder(NasaApodResponse response)
        {
            return response.Title.Equals("NASA Science", StringComparison.OrdinalIgnoreCase)
                || (response.HdUrl?.Contains("nasa-logo", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // removes HTML tags and labels like "Explanation:" or "Image Credit:"
        public static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            string text = Regex.Replace(html, "<.*?>", string.Empty);
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            text = Regex.Replace(text, @"^(Explanation|Credit|Image Credit)[^:]*:\s*", string.Empty, RegexOptions.IgnoreCase);

            return text.Trim();
        }

        public async Task<SpacePicture?> GetPictureAsync(DateOnly date)
        {
            if (!IsDateAvailable(date))
            {
                throw new ArgumentOutOfRangeException(nameof(date),
                    $"NASA's picture of the day is only available from {FirstApodDate:d MMMM yyyy} up to yesterday.");
            }

            // date goes in the URL as yyMMdd, e.g. 050312
            string url = $"wp-json/wp/v2/apod-basic/{date:yyMMdd}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // no picture was posted on that day
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new InvalidOperationException("NASA's request limit has been reached. Please try again later.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"NASA returned {(int)response.StatusCode}.");
            }

            NasaApodResponse? data;
            try
            {
                data = await response.Content.ReadFromJsonAsync<NasaApodResponse>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("NASA sent back data we couldn't read.");
            }

            if (data == null || IsPlaceholder(data) || string.IsNullOrWhiteSpace(data.HdUrl))
            {
                return null;
            }

            bool isVideo = data.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase);

            return new SpacePicture
            {
                Date = date,
                Title = StripHtml(data.Title),
                Explanation = StripHtml(data.Explanation),
                ImageUrl = data.HdUrl,
                VideoUrl = isVideo ? data.Url : null,
                Copyright = StripHtml(data.Copyright ?? data.Credit)
            };
        }
    }
}