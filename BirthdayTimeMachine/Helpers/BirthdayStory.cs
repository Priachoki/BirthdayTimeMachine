namespace BirthdayTimeMachine.Helpers
{
    // which API a bit of the story came from (Plain = normal text)
    public enum StoryPartKind
    {
        Plain,
        City,
        Weather,
        Space,
        Film,
        Holiday,
        History
    }

    public record StoryPart(string Text, StoryPartKind Kind);

    // everything the story needs, taken from the API results
    public class BirthdayStoryInput
    {
        public string? Name { get; init; }
        public DateOnly BirthDate { get; init; }
        public string? City { get; init; }
        public string? WeatherText { get; init; }
        public string? SpaceTitle { get; init; }
        public string? FilmTitle { get; init; }
        public string? HolidayName { get; init; }
        public int HolidayDaysFromBirthday { get; init; }
        public int? HistoryYear { get; init; }
        public string? HistoryText { get; init; }
    }

    // joins the results from all the APIs into one sentence
    public static class BirthdayStory
    {
        public static List<StoryPart> Build(BirthdayStoryInput input)
        {
            var parts = new List<StoryPart>();
            string who = string.IsNullOrWhiteSpace(input.Name) ? "I" : input.Name.Trim();

            parts.Add(Plain($"{who} arrived"));

            if (!string.IsNullOrWhiteSpace(input.City))
            {
                parts.Add(Plain(" in "));
                parts.Add(new StoryPart(input.City, StoryPartKind.City));
            }

            parts.Add(Plain($" on a {input.BirthDate.DayOfWeek}."));

            if (!string.IsNullOrWhiteSpace(input.WeatherText))
            {
                parts.Add(Plain(" The weather was "));
                parts.Add(new StoryPart(input.WeatherText, StoryPartKind.Weather));
                parts.Add(Plain("."));
            }

            if (!string.IsNullOrWhiteSpace(input.SpaceTitle))
            {
                parts.Add(Plain(" NASA's picture of the day was "));
                parts.Add(new StoryPart(input.SpaceTitle, StoryPartKind.Space));
                parts.Add(Plain("."));
            }

            if (!string.IsNullOrWhiteSpace(input.FilmTitle))
            {
                parts.Add(Plain(" "));
                parts.Add(new StoryPart(input.FilmTitle, StoryPartKind.Film));
                parts.Add(Plain(" was in cinemas around then."));
            }

            if (!string.IsNullOrWhiteSpace(input.HolidayName))
            {
                int days = input.HolidayDaysFromBirthday;

                if (days == 0)
                {
                    parts.Add(Plain(" It was "));
                    parts.Add(new StoryPart(input.HolidayName, StoryPartKind.Holiday));
                    parts.Add(Plain(", a public holiday!"));
                }
                else
                {
                    parts.Add(Plain(" "));
                    parts.Add(new StoryPart(input.HolidayName, StoryPartKind.Holiday));
                    parts.Add(Plain($" was {FormatDays(Math.Abs(days))} {(days > 0 ? "later" : "earlier")}."));
                }
            }

            if (input.HistoryYear.HasValue && !string.IsNullOrWhiteSpace(input.HistoryText))
            {
                parts.Add(Plain(" On this day in "));
                parts.Add(new StoryPart(input.HistoryYear.Value.ToString(), StoryPartKind.History));
                parts.Add(Plain($": {EndWithFullStop(input.HistoryText)}"));
            }

            return parts;
        }

        public static string ToPlainText(IEnumerable<StoryPart> parts) =>
            string.Concat(parts.Select(p => p.Text));

        public static string FormatDays(int days) => days == 1 ? "1 day" : $"{days} days";

        public static string GetCssClass(StoryPartKind kind) => kind switch
        {
            StoryPartKind.City => "hl-city",
            StoryPartKind.Weather => "hl-weather",
            StoryPartKind.Space => "hl-space",
            StoryPartKind.Film => "hl-film",
            StoryPartKind.Holiday => "hl-holiday",
            StoryPartKind.History => "hl-history",
            _ => ""
        };

        public static string GetTagClass(StoryPartKind kind) => GetCssClass(kind).Replace("hl-", "tag-");

        public static string GetSourceName(StoryPartKind kind) => kind switch
        {
            StoryPartKind.City => "Place · Open-Meteo",
            StoryPartKind.Weather => "Weather · Open-Meteo",
            StoryPartKind.Space => "Sky · NASA",
            StoryPartKind.Film => "Films · TMDB",
            StoryPartKind.Holiday => "Holiday · Nager.Date",
            StoryPartKind.History => "History · Wikipedia",
            _ => ""
        };

        private static StoryPart Plain(string text) => new(text, StoryPartKind.Plain);

        private static string EndWithFullStop(string text)
        {
            text = text.Trim();
            return text.EndsWith('.') || text.EndsWith('!') || text.EndsWith('?') ? text : text + ".";
        }
    }
}