using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;

namespace BirthdayTimeMachine.Services
{
    // Calls The Movie Database (TMDB) to find films released around a date
    public class MovieService : IMovieService
    {
        public const string PosterBaseUrl = "https://image.tmdb.org/t/p/w342";

        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public MovieService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // key comes from User Secrets ("Tmdb:ApiKey")
            _apiKey = configuration["Tmdb:ApiKey"];
        }

        public async Task<List<Movie>> GetMoviesAroundDateAsync(DateOnly date, int maxItems = 6)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("The movie database key is missing. Add it to User Secrets as Tmdb:ApiKey.");
            }

            if (maxItems <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxItems), "maxItems must be at least 1.");
            }

            // a week either side of the birthday
            DateOnly from = date.AddDays(-7);
            DateOnly to = date.AddDays(7);

            string url = "3/discover/movie" +
                         $"?api_key={Uri.EscapeDataString(_apiKey)}" +
                         $"&primary_release_date.gte={from:yyyy-MM-dd}" +
                         $"&primary_release_date.lte={to:yyyy-MM-dd}" +
                         // vote_count.gte=100 keeps to well-known films and filters out obscure/adult titles
                         "&sort_by=popularity.desc&include_adult=false&vote_count.gte=100&language=en-US";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("The movie database key isn't valid. Check it in User Secrets.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"The movie database returned {(int)response.StatusCode}.");
            }

            TmdbDiscoverResponse? data;
            try
            {
                data = await response.Content.ReadFromJsonAsync<TmdbDiscoverResponse>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The movie database sent back data we couldn't read.");
            }

            if (data?.Results == null)
            {
                return new List<Movie>();
            }

            return data.Results
                .Where(m => !string.IsNullOrWhiteSpace(m.Title))
                .Take(maxItems)
                .Select(ToMovie)
                .ToList();
        }

        // turns TMDB's data into the app's Movie model
        public static Movie ToMovie(TmdbMovie tmdb)
        {
            bool hasDate = DateOnly.TryParseExact(tmdb.ReleaseDate, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly releaseDate);

            return new Movie
            {
                Id = tmdb.Id,
                Title = tmdb.Title,
                Overview = tmdb.Overview ?? string.Empty,
                Rating = tmdb.VoteAverage,
                ReleaseDate = hasDate ? releaseDate : null,
                PosterUrl = string.IsNullOrWhiteSpace(tmdb.PosterPath) ? null : PosterBaseUrl + tmdb.PosterPath,
                TmdbUrl = $"https://www.themoviedb.org/movie/{tmdb.Id}"
            };
        }
    }
}