using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Looks up a city and returns its coordinates
    public interface IGeocodingService
    {
        // returns null if the city isn't found
        Task<GeocodingResult?> GetLocationAsync(string cityName);
    }
}