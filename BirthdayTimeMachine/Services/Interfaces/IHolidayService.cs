using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Finds public holidays on or near a date
    public interface IHolidayService
    {
        // returns null if there's no holiday data for that country
        Task<HolidayResult?> GetHolidayInfoAsync(DateOnly date, string countryCode);
    }
}