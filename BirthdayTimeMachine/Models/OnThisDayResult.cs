namespace BirthdayTimeMachine.Models
{
    // What the app gets back: events and famous people born on that day
    public class OnThisDayResult
    {
        public List<HistoryEvent> Events { get; set; } = new();
        public List<HistoryEvent> Births { get; set; } = new();

        // anything that happened in the exact year the user was born
        public List<HistoryEvent> SameYearEvents { get; set; } = new();
    }
}