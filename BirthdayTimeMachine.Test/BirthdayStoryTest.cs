using BirthdayTimeMachine.Helpers;
using Xunit;

namespace BirthdayTimeMachine.Test
{
    public class BirthdayStoryTests
    {
        // 12 March 2005 was a Saturday
        private static readonly DateOnly Birthday = new(2005, 3, 12);

        private static BirthdayStoryInput FullInput() => new()
        {
            BirthDate = Birthday,
            City = "Dublin",
            WeatherText = "clear sky, 6.9°C",
            SpaceTitle = "Accretion Disk Simulation",
            FilmTitle = "Robots",
            HolidayName = "Saint Patrick's Day",
            HolidayDaysFromBirthday = 5,
            HistoryYear = 1930,
            HistoryText = "Gandhi begins the Salt March"
        };

        [Fact]
        public void Build_WithAllData_HasAPartFromEachApi()
        {
            var kinds = BirthdayStory.Build(FullInput()).Select(p => p.Kind).ToList();

            Assert.Contains(StoryPartKind.City, kinds);
            Assert.Contains(StoryPartKind.Weather, kinds);
            Assert.Contains(StoryPartKind.Space, kinds);
            Assert.Contains(StoryPartKind.Film, kinds);
            Assert.Contains(StoryPartKind.Holiday, kinds);
            Assert.Contains(StoryPartKind.History, kinds);
        }

        [Fact]
        public void Build_WithNoName_StartsWithI()
        {
            string text = BirthdayStory.ToPlainText(BirthdayStory.Build(FullInput()));

            Assert.StartsWith("I arrived in Dublin on a Saturday.", text);
        }

        [Fact]
        public void Build_WithName_UsesTheName()
        {
            var input = new BirthdayStoryInput { Name = "  Priya ", BirthDate = Birthday };

            string text = BirthdayStory.ToPlainText(BirthdayStory.Build(input));

            Assert.Equal("Priya arrived on a Saturday.", text);
        }

        [Fact]
        public void Build_WithOnlyTheDate_StillMakesASentence()
        {
            var parts = BirthdayStory.Build(new BirthdayStoryInput { BirthDate = Birthday });

            Assert.Equal("I arrived on a Saturday.", BirthdayStory.ToPlainText(parts));
            Assert.All(parts, p => Assert.Equal(StoryPartKind.Plain, p.Kind));
        }

        [Fact]
        public void Build_WithNoFilm_LeavesOutTheCinemaPart()
        {
            var input = new BirthdayStoryInput
            {
                BirthDate = Birthday,
                City = "Dublin",
                WeatherText = "clear sky, 6.9°C",
                FilmTitle = null
            };

            var parts = BirthdayStory.Build(input);

            Assert.DoesNotContain(parts, p => p.Kind == StoryPartKind.Film);
            Assert.DoesNotContain("cinemas", BirthdayStory.ToPlainText(parts));
        }

        [Fact]
        public void Build_HolidayOnTheBirthday_SaysItWasAPublicHoliday()
        {
            var input = new BirthdayStoryInput
            {
                BirthDate = Birthday,
                HolidayName = "Saint Patrick's Day",
                HolidayDaysFromBirthday = 0
            };

            string text = BirthdayStory.ToPlainText(BirthdayStory.Build(input));

            Assert.Contains("It was Saint Patrick's Day, a public holiday!", text);
        }

        [Theory]
        [InlineData(5, "Saint Patrick's Day was 5 days later.")]
        [InlineData(-1, "Saint Patrick's Day was 1 day earlier.")]
        public void Build_HolidayNearTheBirthday_SaysHowManyDays(int days, string expected)
        {
            var input = new BirthdayStoryInput
            {
                BirthDate = Birthday,
                HolidayName = "Saint Patrick's Day",
                HolidayDaysFromBirthday = days
            };

            string text = BirthdayStory.ToPlainText(BirthdayStory.Build(input));

            Assert.Contains(expected, text);
        }

        [Fact]
        public void Build_HistoryText_GetsAFullStop()
        {
            string text = BirthdayStory.ToPlainText(BirthdayStory.Build(FullInput()));

            Assert.EndsWith("On this day in 1930: Gandhi begins the Salt March.", text);
        }

        [Fact]
        public void Build_HistoryWithoutYear_IsLeftOut()
        {
            var input = new BirthdayStoryInput { BirthDate = Birthday, HistoryText = "Something happened" };

            var parts = BirthdayStory.Build(input);

            Assert.DoesNotContain(parts, p => p.Kind == StoryPartKind.History);
        }

        [Theory]
        [InlineData(StoryPartKind.City, "hl-city", "tag-city")]
        [InlineData(StoryPartKind.Weather, "hl-weather", "tag-weather")]
        [InlineData(StoryPartKind.Film, "hl-film", "tag-film")]
        [InlineData(StoryPartKind.Plain, "", "")]
        public void CssClasses_MatchTheKind(StoryPartKind kind, string highlight, string tag)
        {
            Assert.Equal(highlight, BirthdayStory.GetCssClass(kind));
            Assert.Equal(tag, BirthdayStory.GetTagClass(kind));
        }

        [Theory]
        [InlineData(1, "1 day")]
        [InlineData(3, "3 days")]
        public void FormatDays_UsesSingularForOne(int days, string expected)
        {
            Assert.Equal(expected, BirthdayStory.FormatDays(days));
        }
    }
}