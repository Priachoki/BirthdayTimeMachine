using System.Net;
using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;

namespace BirthdayTimeMachine.Test
{
    public class OnThisDayServiceTests
    {
        private static readonly DateOnly TestDate = new DateOnly(2005, 3, 12);

        // shaped like Wikipedia's "On this day" feed (shortened sample)
        private const string WikiJson = """
            {
              "events": [
                {
                  "text": "Event in 1990",
                  "year": 1990,
                  "pages": [
                    {
                      "title": "Event_1990",
                      "thumbnail": { "source": "https://upload.wikimedia.org/a.jpg" },
                      "content_urls": { "desktop": { "page": "https://en.wikipedia.org/wiki/Event_1990" } }
                    }
                  ]
                },
                { "text": "Event in 2005", "year": 2005, "pages": [] },
                { "text": "Event in 2004", "year": 2004 },
                { "text": "", "year": 2003 }
              ],
              "births": [
                { "text": "Person born 1950", "year": 1950 },
                { "text": "Person born 2001", "year": 2001 }
              ]
            }
            """;

        [Fact]
        public async Task GetOnThisDayAsync_SortsEventsClosestToBirthYearFirst()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate);

            Assert.NotNull(result);
            Assert.Equal(new int?[] { 2005, 2004, 1990 }, result!.Events.Select(e => e.Year));
        }

        [Fact]
        public async Task GetOnThisDayAsync_SkipsEventsWithNoText()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate);

            Assert.DoesNotContain(result!.Events, e => e.Year == 2003);
        }

        [Fact]
        public async Task GetOnThisDayAsync_MapsImageAndArticleLinkFromFirstPage()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate);
            HistoryEvent event1990 = result!.Events.Single(e => e.Year == 1990);
            HistoryEvent event2004 = result.Events.Single(e => e.Year == 2004);

            Assert.Equal("https://upload.wikimedia.org/a.jpg", event1990.ImageUrl);
            Assert.Equal("https://en.wikipedia.org/wiki/Event_1990", event1990.ArticleUrl);
            Assert.True(event1990.HasImage);
            Assert.False(event2004.HasImage);
        }

        [Fact]
        public async Task GetOnThisDayAsync_FindsEventsFromTheSameYear()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate);

            HistoryEvent sameYear = Assert.Single(result!.SameYearEvents);
            Assert.Equal("Event in 2005", sameYear.Text);
        }

        [Fact]
        public async Task GetOnThisDayAsync_SortsBirthsClosestFirst()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate);

            Assert.Equal(2001, result!.Births.First().Year);
        }

        [Fact]
        public async Task GetOnThisDayAsync_MaxItems_LimitsResults()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            OnThisDayResult? result = await service.GetOnThisDayAsync(TestDate, maxItems: 2);

            Assert.Equal(2, result!.Events.Count);
        }

        [Fact]
        public async Task GetOnThisDayAsync_UsesTwoDigitMonthAndDayInUrl()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            await service.GetOnThisDayAsync(TestDate);

            Assert.EndsWith("onthisday/all/03/12", handler.LastRequestUri!.AbsoluteUri);
        }

        [Fact]
        public async Task GetOnThisDayAsync_ZeroMaxItems_Throws()
        {
            var handler = new FakeHttpMessageHandler(WikiJson);
            var service = new OnThisDayService(handler.CreateClient());

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.GetOnThisDayAsync(TestDate, maxItems: 0));
        }

        [Fact]
        public async Task GetOnThisDayAsync_NotFound_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler("", HttpStatusCode.NotFound);
            var service = new OnThisDayService(handler.CreateClient());

            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetOnThisDayAsync(TestDate));
        }
    }
}