using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;
using System.Net;

namespace BirthdayTimeMachine.Test
{
    public class NasaApodServiceTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2005, 3, 12);

        // shaped like the real science.nasa.gov apod-basic response, including its HTML
        private const string ApodJson = """
            {
              "date": "2005-03-12",
              "title": "Accretion Disk Simulation",
              "media_type": "image",
              "explanation": "<strong>Explanation:</strong> Don't be fooled by the familiar pattern.",
              "url": "https://science.nasa.gov/image-article/apod-2005-march-12-accretion-disk-simulation/",
              "hdurl": "https://assets.science.nasa.gov/content/dam/science/cds/apod/apod/2005/march/accretion_mpowen_full.jpg",
              "copyright": "<a href=\"https://example.com\">Michael Owen</a>, John Blondin",
              "credit": null
            }
            """;

        [Fact]
        public async Task GetPictureAsync_ValidResponse_MapsAndCleansFields()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(ApodJson);
            var service = new NasaApodService(handler.CreateClient());

            // Act
            SpacePicture? result = await service.GetPictureAsync(TestDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Accretion Disk Simulation", result!.Title);
            Assert.Equal("Don't be fooled by the familiar pattern.", result.Explanation);
            Assert.Equal("Michael Owen, John Blondin", result.Copyright);
            Assert.EndsWith("accretion_mpowen_full.jpg", result.ImageUrl);
            Assert.False(result.IsVideo);
        }

        [Fact]
        public async Task GetPictureAsync_VideoDay_SetsVideoUrl()
        {
            string json = """
                {
                  "title": "A Video Day",
                  "media_type": "video",
                  "explanation": "A video.",
                  "url": "https://science.nasa.gov/image-article/apod-video/",
                  "hdurl": "https://assets.science.nasa.gov/content/dam/science/cds/apod/apod/2010/thumb.jpg"
                }
                """;
            var handler = new FakeHttpMessageHandler(json);
            var service = new NasaApodService(handler.CreateClient());

            SpacePicture? result = await service.GetPictureAsync(TestDate);

            Assert.True(result!.IsVideo);
            Assert.Equal("https://science.nasa.gov/image-article/apod-video/", result.VideoUrl);
        }

        [Fact]
        public async Task GetPictureAsync_PlaceholderImage_ReturnsNull()
        {
            // what NASA sends for some dates, e.g. 1 January 2000
            string json = """
                {
                  "title": "The Millennium that Defines Universe",
                  "media_type": "image",
                  "explanation": "Welcome to the millennial year.",
                  "hdurl": "https://assets.science.nasa.gov/dynamicimage/assets/science/astro/images/misc/news-thumbnail.png"
                }
                """;
            var handler = new FakeHttpMessageHandler(json);
            var service = new NasaApodService(handler.CreateClient());

            SpacePicture? result = await service.GetPictureAsync(new DateOnly(2000, 1, 1));

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPictureAsync_NotFound_ReturnsNull()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.NotFound);
            var service = new NasaApodService(handler.CreateClient());

            SpacePicture? result = await service.GetPictureAsync(TestDate);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPictureAsync_TooManyRequests_ThrowsInvalidOperationException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.TooManyRequests);
            var service = new NasaApodService(handler.CreateClient());

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetPictureAsync(TestDate));
        }

        [Fact]
        public async Task GetPictureAsync_DateBeforeApodStarted_ThrowsWithoutCallingApi()
        {
            var handler = new FakeHttpMessageHandler(ApodJson);
            var service = new NasaApodService(handler.CreateClient());

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.GetPictureAsync(new DateOnly(1990, 1, 1)));
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetPictureAsync_PutsDateInUrlAsYyMmDd()
        {
            var handler = new FakeHttpMessageHandler(ApodJson);
            var service = new NasaApodService(handler.CreateClient());

            await service.GetPictureAsync(TestDate);

            Assert.EndsWith("apod-basic/050312", handler.LastRequestUri!.AbsoluteUri);
        }

        [Theory]
        [InlineData(1995, 6, 16, true)]
        [InlineData(1995, 6, 15, false)]
        [InlineData(2005, 3, 12, true)]
        public void IsDateAvailable_ChecksApodStartDate(int year, int month, int day, bool expected)
        {
            Assert.Equal(expected, NasaApodService.IsDateAvailable(new DateOnly(year, month, day)));
        }

        [Fact]
        public void StripHtml_RemovesTagsLabelsAndDecodesEntities()
        {
            string result = NasaApodService.StripHtml("<strong>Explanation:</strong> Stars &amp; galaxies");

            Assert.Equal("Stars & galaxies", result);
        }

        [Fact]
        public void StripHtml_Null_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, NasaApodService.StripHtml(null));
        }

        [Fact]
        public void GetOfficialPageUrl_FormatsDateCorrectly()
        {
            Assert.Equal("https://apod.nasa.gov/apod/ap050312.html", NasaApodService.GetOfficialPageUrl(TestDate));
        }
    }
}