using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;
using System.Net;

namespace BirthdayTimeMachine.Test
{
    public class HolidayServiceTests
    {
        // shaped like Nager.Date's PublicHolidays response (shortened sample)
        private const string IrelandJson = """
            [
              { "date": "2005-01-01", "localName": "Lá Caille", "name": "New Year's Day", "countryCode": "IE" },
              { "date": "2005-03-17", "localName": "Lá Fhéile Pádraig", "name": "Saint Patrick's Day", "countryCode": "IE" },
              { "date": "2005-12-25", "localName": "Lá Nollag", "name": "Christmas Day", "countryCode": "IE" }
            ]
            """;

        private static PublicHoliday Holiday(string date, string name)
        {
            return new PublicHoliday { DateText = date, Name = name };
        }

        //  FindClosest logic 

        [Fact]
        public void FindClosest_BirthdayIsAHoliday_ReturnsIsOnBirthday()
        {
            var holidays = new List<PublicHoliday> { Holiday("2000-03-17", "Saint Patrick's Day") };

            HolidayResult? result = HolidayService.FindClosest(holidays, new DateOnly(2000, 3, 17));

            Assert.NotNull(result);
            Assert.True(result!.IsOnBirthday);
            Assert.Equal(0, result.DaysFromBirthday);
        }

        [Fact]
        public void FindClosest_HolidayAfterBirthday_ReturnsPositiveDays()
        {
            var holidays = new List<PublicHoliday>
            {
                Holiday("2005-01-01", "New Year's Day"),
                Holiday("2005-03-17", "Saint Patrick's Day")
            };

            HolidayResult? result = HolidayService.FindClosest(holidays, new DateOnly(2005, 3, 12));

            Assert.Equal("Saint Patrick's Day", result!.Holiday.Name);
            Assert.Equal(5, result.DaysFromBirthday);
            Assert.False(result.IsOnBirthday);
        }

        [Fact]
        public void FindClosest_HolidayBeforeBirthday_ReturnsNegativeDays()
        {
            var holidays = new List<PublicHoliday> { Holiday("2005-12-25", "Christmas Day") };

            HolidayResult? result = HolidayService.FindClosest(holidays, new DateOnly(2005, 12, 28));

            Assert.Equal(-3, result!.DaysFromBirthday);
        }

        [Fact]
        public void FindClosest_NoHolidays_ReturnsNull()
        {
            Assert.Null(HolidayService.FindClosest(new List<PublicHoliday>(), new DateOnly(2005, 3, 12)));
        }

        [Fact]
        public void FindClosest_IgnoresHolidaysWithBadDates()
        {
            var holidays = new List<PublicHoliday> { Holiday("not-a-date", "Broken") };

            Assert.Null(HolidayService.FindClosest(holidays, new DateOnly(2005, 3, 12)));
        }

        // API mapping 

        [Fact]
        public async Task GetHolidayInfoAsync_ValidResponse_MapsAndFindsClosest()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(IrelandJson);
            var service = new HolidayService(handler.CreateClient());

            // Act
            HolidayResult? result = await service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "IE");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Saint Patrick's Day", result!.Holiday.Name);
            Assert.Equal("Lá Fhéile Pádraig", result.Holiday.LocalName);
            Assert.Equal(new DateOnly(2005, 3, 17), result.Holiday.Date);
            Assert.Equal(5, result.DaysFromBirthday);
        }

        [Fact]
        public async Task GetHolidayInfoAsync_BuildsUrlWithYearAndUppercaseCountry()
        {
            var handler = new FakeHttpMessageHandler(IrelandJson);
            var service = new HolidayService(handler.CreateClient());

            await service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "ie");

            Assert.EndsWith("api/v3/PublicHolidays/2005/IE", handler.LastRequestUri!.AbsoluteUri);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task GetHolidayInfoAsync_CountryNotSupported_ReturnsNull(HttpStatusCode status)
        {
            var handler = new FakeHttpMessageHandler("", status);
            var service = new HolidayService(handler.CreateClient());

            HolidayResult? result = await service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "XX");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetHolidayInfoAsync_EmptyCountryCode_ReturnsNullWithoutCallingApi()
        {
            var handler = new FakeHttpMessageHandler(IrelandJson);
            var service = new HolidayService(handler.CreateClient());

            HolidayResult? result = await service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "");

            Assert.Null(result);
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetHolidayInfoAsync_InvalidJson_ThrowsInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler("[ not json");
            var service = new HolidayService(handler.CreateClient());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "IE"));
        }

        [Fact]
        public async Task GetHolidayInfoAsync_ServerError_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = new HolidayService(handler.CreateClient());

            await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetHolidayInfoAsync(new DateOnly(2005, 3, 12), "IE"));
        }
    }
}