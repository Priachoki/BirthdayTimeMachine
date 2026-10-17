using System.Net;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;

namespace BirthdayTimeMachine.Test
{
    public class GeocodingServiceTests
    {
        // shaped like a real Open-Meteo geocoding response
        private const string DublinJson = """
            {
              "results": [
                {
                  "name": "Dublin",
                  "latitude": 53.33306,
                  "longitude": -6.24889,
                  "country_code": "IE",
                  "country": "Ireland",
                  "admin1": "Leinster",
                  "timezone": "Europe/Dublin"
                }
              ]
            }
            """;

        [Fact]
        public async Task GetLocationAsync_ValidResponse_MapsAllFields()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(DublinJson);
            var service = new GeocodingService(handler.CreateClient());

            // Act
            GeocodingResult? result = await service.GetLocationAsync("Dublin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dublin", result!.Name);
            Assert.Equal(53.33306, result.Latitude);
            Assert.Equal(-6.24889, result.Longitude);
            Assert.Equal("IE", result.CountryCode);
            Assert.Equal("Ireland", result.Country);
            Assert.Equal("Leinster", result.Region);
            Assert.Equal("Dublin, Leinster, Ireland", result.DisplayName);
        }

        [Fact]
        public async Task GetLocationAsync_NoResults_ReturnsNull()
        {
            // the real API leaves out "results" when a city isn't found
            var handler = new FakeHttpMessageHandler("""{ "generationtime_ms": 0.4 }""");
            var service = new GeocodingService(handler.CreateClient());

            GeocodingResult? result = await service.GetLocationAsync("Xyzabc");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLocationAsync_ServerError_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = new GeocodingService(handler.CreateClient());

            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetLocationAsync("Dublin"));
        }

        [Fact]
        public async Task GetLocationAsync_InvalidJson_ThrowsInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler("this is not json");
            var service = new GeocodingService(handler.CreateClient());

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetLocationAsync("Dublin"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetLocationAsync_EmptyCity_ThrowsWithoutCallingApi(string city)
        {
            var handler = new FakeHttpMessageHandler(DublinJson);
            var service = new GeocodingService(handler.CreateClient());

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetLocationAsync(city));
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetLocationAsync_CityWithSpace_IsEncodedInUrl()
        {
            var handler = new FakeHttpMessageHandler(DublinJson);
            var service = new GeocodingService(handler.CreateClient());

            await service.GetLocationAsync("New York");

            Assert.Contains("name=New%20York", handler.LastRequestUri!.AbsoluteUri);
        }

        [Fact]
        public void DisplayName_NoRegion_LeavesRegionOut()
        {
            var result = new GeocodingResult { Name = "Monaco", Country = "Monaco", Region = null };

            Assert.Equal("Monaco, Monaco", result.DisplayName);
        }
    }
}