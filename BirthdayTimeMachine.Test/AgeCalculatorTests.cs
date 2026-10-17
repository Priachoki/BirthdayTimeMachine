using BirthdayTimeMachine.Helpers;
using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Test
{
    public class AgeCalculatorTests
    {
        private readonly DateOnly _today = new DateOnly(2026, 10, 17);

        [Fact]
        public void GetAgeInYears_BirthdayAlreadyPassedThisYear_ReturnsFullYears()
        {
            int age = AgeCalculator.GetAgeInYears(new DateOnly(2000, 3, 12), _today);

            Assert.Equal(26, age);
        }

        [Fact]
        public void GetAgeInYears_BirthdayNotYetThisYear_ReturnsOneLess()
        {
            int age = AgeCalculator.GetAgeInYears(new DateOnly(2000, 12, 25), _today);

            Assert.Equal(25, age);
        }

        [Fact]
        public void Calculate_BirthdayIsToday_ReturnsZeroDaysAndIsBirthdayToday()
        {
            AgeSummary result = AgeCalculator.Calculate(new DateOnly(2000, 10, 17), _today);

            Assert.Equal(26, result.Years);
            Assert.Equal(0, result.DaysUntilNextBirthday);
            Assert.True(result.IsBirthdayToday);
        }

        [Fact]
        public void GetNextBirthday_BirthdayAlreadyPassed_ReturnsNextYear()
        {
            DateOnly next = AgeCalculator.GetNextBirthday(new DateOnly(2000, 3, 12), _today);

            Assert.Equal(new DateOnly(2027, 3, 12), next);
        }

        [Fact]
        public void Calculate_BirthdayInThreeDays_ReturnsThreeDaysUntil()
        {
            AgeSummary result = AgeCalculator.Calculate(new DateOnly(2000, 10, 20), _today);

            Assert.Equal(3, result.DaysUntilNextBirthday);
        }

        [Fact]
        public void GetNextBirthday_LeapDayBirthdayInNonLeapYear_Returns28February()
        {
            DateOnly next = AgeCalculator.GetNextBirthday(new DateOnly(2004, 2, 29), _today);

            Assert.Equal(new DateOnly(2027, 2, 28), next);
        }

        [Fact]
        public void GetNextBirthday_LeapDayBirthdayBeforeLeapYear_Returns29February()
        {
            DateOnly today = new DateOnly(2027, 10, 17);

            DateOnly next = AgeCalculator.GetNextBirthday(new DateOnly(2004, 2, 29), today);

            Assert.Equal(new DateOnly(2028, 2, 29), next);
        }

        [Fact]
        public void Calculate_BornOneWeekAgo_ReturnsSevenDaysAndOneWeek()
        {
            AgeSummary result = AgeCalculator.Calculate(new DateOnly(2026, 10, 10), _today);

            Assert.Equal(7, result.TotalDays);
            Assert.Equal(1, result.TotalWeeks);
        }

        [Fact]
        public void Calculate_KnownDate_ReturnsCorrectWeekday()
        {
            // 1 March 2001 was a Thursday
            AgeSummary result = AgeCalculator.Calculate(new DateOnly(2001, 3, 1), _today);

            Assert.Equal(DayOfWeek.Thursday, result.BornOnWeekday);
        }

        [Fact]
        public void Calculate_FutureBirthDate_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                AgeCalculator.Calculate(new DateOnly(2030, 1, 1), _today));
        }
    }
}