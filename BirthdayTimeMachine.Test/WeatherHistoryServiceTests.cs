using System.Net;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;

namespace BirthdayTimeMachine.Test
{
    public class WeatherHistoryServiceTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2005, 3, 12);

        // shaped like a real Open-Meteo archive response (values are sample data)
        private const string WeatherJson = """
            {
              "daily": {
                "time": ["2005-03-12"],
                "weather_code": [3],
                "temperature_2m_max": [6.9],
                "temperature_2m_min": [1.2],
                "precipitation_sum": [0.0],
                "wind_speed_10m_max": [14.5]
              }
            }
            """;

        [Fact]
        public async Task GetWeatherForDateAsync_ValidResponse_MapsListsIntoWeatherDay()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(WeatherJson);
            var service = new WeatherHistoryService(handler.CreateClient());

            // Act
            WeatherDay? result = await service.GetWeatherForDateAsync(53.35, -6.26, TestDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDate, result!.Date);
            Assert.Equal(3, result.WeatherCode);
            Assert.Equal(6.9, result.MaxTemperature);
            Assert.Equal(1.2, result.MinTemperature);
            Assert.Equal(0.0, result.Precipitation);
            Assert.Equal(14.5, result.MaxWindSpeed);
            Assert.False(result.WasRainy);
        }

        [Fact]
        public async Task GetWeatherForDateAsync_MissingValues_MapToNull()
        {
            string json = """
                {
                  "daily": {
                    "time": ["2005-03-12"],
                    "weather_code": [null],
                    "temperature_2m_max": [null],
                    "temperature_2m_min": [null],
                    "precipitation_sum": [null],
                    "wind_speed_10m_max": [null]
                  }
                }
                """;
            var handler = new FakeHttpMessageHandler(json);
            var service = new WeatherHistoryService(handler.CreateClient());

            WeatherDay? result = await service.GetWeatherForDateAsync(53.35, -6.26, TestDate);

            Assert.NotNull(result);
            Assert.Null(result!.MaxTemperature);
            Assert.Null(result.WeatherCode);
        }

        [Fact]
        public async Task GetWeatherForDateAsync_NoDays_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler("""{ "daily": { "time": [] } }""");
            var service = new WeatherHistoryService(handler.CreateClient());

            WeatherDay? result = await service.GetWeatherForDateAsync(53.35, -6.26, TestDate);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeatherForDateAsync_BuildsUrlWithCoordinatesAndDate()
        {
            var handler = new FakeHttpMessageHandler(WeatherJson);
            var service = new WeatherHistoryService(handler.CreateClient());

            await service.GetWeatherForDateAsync(53.35, -6.26, TestDate);

            string url = handler.LastRequestUri!.AbsoluteUri;
            Assert.Contains("latitude=53.35", url);
            Assert.Contains("longitude=-6.26", url);
            Assert.Contains("start_date=2005-03-12", url);
        }

        [Fact]
        public async Task GetWeatherForDateAsync_DateBefore1940_ThrowsWithoutCallingApi()
        {
            var handler = new FakeHttpMessageHandler(WeatherJson);
            var service = new WeatherHistoryService(handler.CreateClient());

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.GetWeatherForDateAsync(53.35, -6.26, new DateOnly(1930, 1, 1)));
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetWeatherForDateAsync_FutureDate_Throws()
        {
            var handler = new FakeHttpMessageHandler(WeatherJson);
            var service = new WeatherHistoryService(handler.CreateClient());
            DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.GetWeatherForDateAsync(53.35, -6.26, tomorrow));
        }

        [Fact]
        public async Task GetWeatherForDateAsync_ServerError_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.BadGateway);
            var service = new WeatherHistoryService(handler.CreateClient());

            await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetWeatherForDateAsync(53.35, -6.26, TestDate));
        }

        [Fact]
        public void WasRainy_MoreThanOneMillimetre_ReturnsTrue()
        {
            var day = new WeatherDay { Precipitation = 5.2 };

            Assert.True(day.WasRainy);
        }
    }
}