namespace BirthdayTimeMachine.Helpers
{
    // Turns Open-Meteo weather codes into text and a Bootstrap icon
    // codes: https://open-meteo.com/en/docs (WMO Weather interpretation codes)
    // icons: https://icons.getbootstrap.com
    public static class WeatherCodeMapper
    {
        private static readonly Dictionary<int, (string Description, string IconClass)> Codes = new()
        {
            { 0,  ("Clear sky", "bi-sun") },
            { 1,  ("Mainly clear", "bi-cloud-sun") },
            { 2,  ("Partly cloudy", "bi-cloud-sun") },
            { 3,  ("Overcast", "bi-clouds") },
            { 45, ("Fog", "bi-cloud-fog") },
            { 48, ("Freezing fog", "bi-cloud-fog2") },
            { 51, ("Light drizzle", "bi-cloud-drizzle") },
            { 53, ("Drizzle", "bi-cloud-drizzle") },
            { 55, ("Heavy drizzle", "bi-cloud-drizzle") },
            { 56, ("Freezing drizzle", "bi-cloud-sleet") },
            { 57, ("Heavy freezing drizzle", "bi-cloud-sleet") },
            { 61, ("Light rain", "bi-cloud-rain") },
            { 63, ("Rain", "bi-cloud-rain") },
            { 65, ("Heavy rain", "bi-cloud-rain-heavy") },
            { 66, ("Freezing rain", "bi-cloud-sleet") },
            { 67, ("Heavy freezing rain", "bi-cloud-sleet") },
            { 71, ("Light snow", "bi-cloud-snow") },
            { 73, ("Snow", "bi-cloud-snow") },
            { 75, ("Heavy snow", "bi-snow") },
            { 77, ("Snow grains", "bi-cloud-snow") },
            { 80, ("Light showers", "bi-cloud-rain") },
            { 81, ("Showers", "bi-cloud-rain") },
            { 82, ("Violent showers", "bi-cloud-rain-heavy") },
            { 85, ("Snow showers", "bi-cloud-snow") },
            { 86, ("Heavy snow showers", "bi-snow") },
            { 95, ("Thunderstorm", "bi-cloud-lightning-rain") },
            { 96, ("Thunderstorm with hail", "bi-cloud-hail") },
            { 99, ("Severe thunderstorm with hail", "bi-cloud-hail") }
        };

        public static string GetDescription(int? code)
        {
            if (code.HasValue && Codes.TryGetValue(code.Value, out var info))
            {
                return info.Description;
            }
            return "Unknown";
        }

        public static string GetIconClass(int? code)
        {
            if (code.HasValue && Codes.TryGetValue(code.Value, out var info))
            {
                return info.IconClass;
            }
            return "bi-question-circle";
        }
    }
}