namespace BirthdayTimeMachine.Models
{
    // Facts about the user's age, worked out from their birth date
    public class AgeSummary
    {
        public int Years { get; set; }
        public int TotalDays { get; set; }
        public DayOfWeek BornOnWeekday { get; set; }
        public DateOnly NextBirthday { get; set; }
        public int DaysUntilNextBirthday { get; set; }

        public int TotalWeeks => TotalDays / 7;
        public bool IsBirthdayToday => DaysUntilNextBirthday == 0;
    }
}