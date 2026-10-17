using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Saves and loads birthday reports
    public interface IReportStorageService
    {
        Task<List<SavedReport>> GetAllAsync();
        Task SaveAsync(SavedReport report);
        Task<bool> DeleteAsync(Guid id);
    }
}