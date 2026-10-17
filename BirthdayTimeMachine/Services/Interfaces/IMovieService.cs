using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Finds popular films released around a date
    public interface IMovieService
    {
        Task<List<Movie>> GetMoviesAroundDateAsync(DateOnly date, int maxItems = 6);
    }
}