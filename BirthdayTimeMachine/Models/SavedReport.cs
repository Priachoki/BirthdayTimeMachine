using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // A birthday report the user has saved (stored in Data/reports.json)
    public class SavedReport : IComparable<SavedReport>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;

        public double? MaxTemperature { get; set; }
        public double? MinTemperature { get; set; }
        public double? Precipitation { get; set; }
        public double? MaxWindSpeed { get; set; }
        public int? WeatherCode { get; set; }

        public string? SpaceTitle { get; set; }
        public string? SpaceImageUrl { get; set; }
        public string? HighlightEvent { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;

        // worked out from Precipitation, so it doesn't need saving
        [JsonIgnore]
        public bool WasRainy => Precipitation > 1.0;

        // default sort order: oldest birthday first, then by name
        public int CompareTo(SavedReport? other)
        {
            if (other == null)
            {
                return 1;
            }

            int result = BirthDate.CompareTo(other.BirthDate);

            return result != 0
                ? result
                : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}