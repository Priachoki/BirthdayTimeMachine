using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Gets historical events and famous birthdays for a date
    public interface IOnThisDayService
    {
        Task<OnThisDayResult?> GetOnThisDayAsync(DateOnly date, int maxItems = 5);
    }
}