namespace BirthdayTimeMachine.Models
{
    // The weather for one day, in a simpler shape for the app to use
    public class WeatherDay
    {
        public DateOnly Date { get; set; }
        public int? WeatherCode { get; set; }
        public double? MaxTemperature { get; set; }   // °C
        public double? MinTemperature { get; set; }   // °C
        public double? Precipitation { get; set; }    // mm
        public double? MaxWindSpeed { get; set; }     // km/h

        public bool WasRainy => Precipitation > 1.0;
    }
}