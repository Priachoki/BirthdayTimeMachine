using System.Globalization;
using System.Text.Json.Serialization;

namespace BirthdayTimeMachine.Models
{
    // One public holiday from Nager.Date
    // https://date.nager.at/Api
    public class PublicHoliday
    {
        // e.g. "2005-03-17"
        [JsonPropertyName("date")]
        public string DateText { get; set; } = string.Empty;

        // name in the local language, e.g. "Lá Fhéile Pádraig"
        [JsonPropertyName("localName")]
        public string LocalName { get; set; } = string.Empty;

        // English name, e.g. "Saint Patrick's Day"
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonIgnore]
        public DateOnly? Date =>
            DateOnly.TryParseExact(DateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
                ? date
                : null;
    }
}