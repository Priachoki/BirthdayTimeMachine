using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Gets the weather for a place on a past date
    public interface IWeatherHistoryService
    {
        // returns null if there's no data for that day
        Task<WeatherDay?> GetWeatherForDateAsync(double latitude, double longitude, DateOnly date);
    }
}