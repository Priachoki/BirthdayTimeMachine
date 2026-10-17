namespace BirthdayTimeMachine.Models
{
    // The public holiday on (or closest to) the user's birthday
    public class HolidayResult
    {
        public PublicHoliday Holiday { get; set; } = new();
        public bool IsOnBirthday { get; set; }

        // positive = holiday was after the birthday, negative = before
        public int DaysFromBirthday { get; set; }
    }
}