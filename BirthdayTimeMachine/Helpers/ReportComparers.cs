using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Helpers
{
    // Sorts reports by the highest temperature on the birthday.
    // Reports with no temperature always go to the end.
    public class TemperatureComparer : IComparer<SavedReport>
    {
        private readonly bool _warmestFirst;

        public TemperatureComparer(bool warmestFirst = false)
        {
            _warmestFirst = warmestFirst;
        }

        public int Compare(SavedReport? x, SavedReport? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            if (x.MaxTemperature == null && y.MaxTemperature == null) return 0;
            if (x.MaxTemperature == null) return 1;
            if (y.MaxTemperature == null) return -1;

            int result = x.MaxTemperature.Value.CompareTo(y.MaxTemperature.Value);
            return _warmestFirst ? -result : result;
        }
    }

    // Sorts reports alphabetically by name, then by birth date
    public class NameComparer : IComparer<SavedReport>
    {
        public int Compare(SavedReport? x, SavedReport? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int result = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            return result != 0 ? result : x.BirthDate.CompareTo(y.BirthDate);
        }
    }
}