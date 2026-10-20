using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BirthdayTimeMachine.Test
{
    public class MovieServiceTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2003, 1, 1);

        // shaped like TMDB's discover/movie response (shortened sample)
        private const string MoviesJson = """
            {
              "page": 1,
              "results": [
                {
                  "id": 4912,
                  "title": "Confessions of a Dangerous Mind",
                  "release_date": "2002-12-31",
                  "overview": "A TV producer with a secret life.",
                  "poster_path": "/poster1.jpg",
                  "vote_average": 6.74
                },
                {
                  "id": 1000,
                  "title": "No Poster Film",
                  "release_date": "",
                  "overview": null,
                  "poster_path": null,
                  "vote_average": 5.0
                },
                {
                  "id": 2000,
                  "title": "Third Film",
                  "release_date": "2003-01-03",
                  "poster_path": "/poster3.jpg",
                  "vote_average": 7.1
                }
              ]
            }
            """;

        private static IConfiguration ConfigWithKey(string? key = "test-key")
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Tmdb:ApiKey"] = key })
                .Build();
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_ValidResponse_MapsFields()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(MoviesJson);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            // Act
            List<Movie> movies = await service.GetMoviesAroundDateAsync(TestDate);

            // Assert
            Movie first = movies[0];
            Assert.Equal(4912, first.Id);
            Assert.Equal("Confessions of a Dangerous Mind", first.Title);
            Assert.Equal(new DateOnly(2002, 12, 31), first.ReleaseDate);
            Assert.Equal("https://image.tmdb.org/t/p/w342/poster1.jpg", first.PosterUrl);
            Assert.Equal("https://www.themoviedb.org/movie/4912", first.TmdbUrl);
            Assert.Equal("6.7", first.RatingText);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_MissingPosterAndDate_MapToNull()
        {
            var handler = new FakeHttpMessageHandler(MoviesJson);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            List<Movie> movies = await service.GetMoviesAroundDateAsync(TestDate);
            Movie noPoster = movies.Single(m => m.Id == 1000);

            Assert.Null(noPoster.PosterUrl);
            Assert.Null(noPoster.ReleaseDate);
            Assert.Equal(string.Empty, noPoster.Overview);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_MaxItems_LimitsResults()
        {
            var handler = new FakeHttpMessageHandler(MoviesJson);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            List<Movie> movies = await service.GetMoviesAroundDateAsync(TestDate, maxItems: 2);

            Assert.Equal(2, movies.Count);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_BuildsUrlWithDateRangeAndFilters()
        {
            var handler = new FakeHttpMessageHandler(MoviesJson);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            await service.GetMoviesAroundDateAsync(TestDate);

            string url = handler.LastRequestUri!.AbsoluteUri;
            Assert.Contains("primary_release_date.gte=2002-12-25", url);
            Assert.Contains("primary_release_date.lte=2003-01-08", url);
            Assert.Contains("include_adult=false", url);
            Assert.Contains("vote_count.gte=100", url);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_NoResults_ReturnsEmptyList()
        {
            var handler = new FakeHttpMessageHandler("""{ "page": 1, "results": [] }""");
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            List<Movie> movies = await service.GetMoviesAroundDateAsync(TestDate);

            Assert.Empty(movies);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_MissingKey_ThrowsWithoutCallingApi()
        {
            var handler = new FakeHttpMessageHandler(MoviesJson);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey(null));

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetMoviesAroundDateAsync(TestDate));
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_InvalidKey_ThrowsInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.Unauthorized);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetMoviesAroundDateAsync(TestDate));
        }

        [Fact]
        public async Task GetMoviesAroundDateAsync_ServerError_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.ServiceUnavailable);
            var service = new MovieService(handler.CreateClient(), ConfigWithKey());

            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetMoviesAroundDateAsync(TestDate));
        }
    }
}