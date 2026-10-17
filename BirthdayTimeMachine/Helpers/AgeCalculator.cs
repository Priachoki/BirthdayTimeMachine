using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Helpers
{
    // Works out age, days lived and the countdown to the next birthday.
    // "today" is passed in instead of using DateTime.Today so the methods can be unit tested.
    public static class AgeCalculator
    {
        public static AgeSummary Calculate(DateOnly birthDate, DateOnly today)
        {
            if (birthDate > today)
            {
                throw new ArgumentException("Date of birth can't be in the future.", nameof(birthDate));
            }

            DateOnly nextBirthday = GetNextBirthday(birthDate, today);

            return new AgeSummary
            {
                Years = GetAgeInYears(birthDate, today),
                TotalDays = today.DayNumber - birthDate.DayNumber,
                BornOnWeekday = birthDate.DayOfWeek,
                NextBirthday = nextBirthday,
                DaysUntilNextBirthday = nextBirthday.DayNumber - today.DayNumber
            };
        }

        public static int GetAgeInYears(DateOnly birthDate, DateOnly today)
        {
            int years = today.Year - birthDate.Year;

            // birthday hasn't happened yet this year
            if (today < BirthdayInYear(birthDate, today.Year))
            {
                years--;
            }

            return years;
        }

        public static DateOnly GetNextBirthday(DateOnly birthDate, DateOnly today)
        {
            DateOnly thisYear = BirthdayInYear(birthDate, today.Year);

            return thisYear >= today
                ? thisYear
                : BirthdayInYear(birthDate, today.Year + 1);
        }

        // 29 February birthdays are celebrated on 28 February in non-leap years
        public static DateOnly BirthdayInYear(DateOnly birthDate, int year)
        {
            if (birthDate.Month == 2 && birthDate.Day == 29 && !DateTime.IsLeapYear(year))
            {
                return new DateOnly(year, 2, 28);
            }

            return new DateOnly(year, birthDate.Month, birthDate.Day);
        }
    }
}