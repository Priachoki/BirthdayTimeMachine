using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Services.Interfaces
{
    // Gets NASA's Astronomy Picture of the Day for a date
    public interface INasaApodService
    {
        // returns null if NASA has no usable picture for that date
        Task<SpacePicture?> GetPictureAsync(DateOnly date);
    }
}