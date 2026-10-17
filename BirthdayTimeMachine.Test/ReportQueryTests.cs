using BirthdayTimeMachine.Helpers;
using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Test
{
    public class ReportQueryTests
    {
        private static SavedReport Report(string name, int year, double? temp = null, double? rain = null,
                                          string city = "Dublin", string? spaceImage = null, int savedMinutesAgo = 0)
        {
            return new SavedReport
            {
                Name = name,
                BirthDate = new DateOnly(year, 1, 1),
                City = city,
                MaxTemperature = temp,
                Precipitation = rain,
                SpaceImageUrl = spaceImage,
                SavedAt = new DateTime(2026, 10, 17, 12, 0, 0).AddMinutes(-savedMinutesAgo)
            };
        }

        // ---------- sorting ----------

        [Fact]
        public void Sort_OldestBirthday_UsesIComparable()
        {
            var reports = new List<SavedReport> { Report("A", 2005), Report("B", 1994), Report("C", 2003) };

            var result = ReportQuery.Sort(reports, ReportSortOption.OldestBirthday);

            Assert.Equal(new[] { "B", "C", "A" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_NewestBirthday_ReversesOrder()
        {
            var reports = new List<SavedReport> { Report("A", 2005), Report("B", 1994), Report("C", 2003) };

            var result = ReportQuery.Sort(reports, ReportSortOption.NewestBirthday);

            Assert.Equal(new[] { "A", "C", "B" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_Warmest_PutsMissingTemperatureLast()
        {
            var reports = new List<SavedReport> { Report("Cold", 2000, 10), Report("None", 2000), Report("Hot", 2000, 24) };

            var result = ReportQuery.Sort(reports, ReportSortOption.Warmest);

            Assert.Equal(new[] { "Hot", "Cold", "None" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_Coldest_PutsMissingTemperatureLast()
        {
            var reports = new List<SavedReport> { Report("Hot", 2000, 24), Report("None", 2000), Report("Cold", 2000, 10) };

            var result = ReportQuery.Sort(reports, ReportSortOption.Coldest);

            Assert.Equal(new[] { "Cold", "Hot", "None" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_Name_IgnoresCase()
        {
            var reports = new List<SavedReport> { Report("bob", 2000), Report("Alice", 2000), Report("carl", 2000) };

            var result = ReportQuery.Sort(reports, ReportSortOption.Name);

            Assert.Equal(new[] { "Alice", "bob", "carl" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_RecentlySaved_NewestSaveFirst()
        {
            var reports = new List<SavedReport>
            {
                Report("Old", 2000, savedMinutesAgo: 60),
                Report("New", 2000, savedMinutesAgo: 1),
                Report("Middle", 2000, savedMinutesAgo: 30)
            };

            var result = ReportQuery.Sort(reports, ReportSortOption.RecentlySaved);

            Assert.Equal(new[] { "New", "Middle", "Old" }, result.Select(r => r.Name));
        }

        [Fact]
        public void Sort_DoesNotChangeTheOriginalList()
        {
            var reports = new List<SavedReport> { Report("A", 2005), Report("B", 1994) };

            ReportQuery.Sort(reports, ReportSortOption.OldestBirthday);

            Assert.Equal("A", reports[0].Name);
        }

        [Fact]
        public void CompareTo_SameBirthDate_SortsByName()
        {
            var reports = new List<SavedReport> { Report("Zoe", 2000), Report("Amy", 2000) };

            reports.Sort();

            Assert.Equal("Amy", reports[0].Name);
        }

        // ---------- filtering ----------

        [Fact]
        public void Filter_Rainy_OnlyReportsWithMoreThanOneMillimetre()
        {
            var reports = new List<SavedReport> { Report("Wet", 2000, rain: 5.0), Report("Damp", 2000, rain: 0.2), Report("Unknown", 2000) };

            var result = ReportQuery.Filter(reports, ReportFilter.Rainy);

            Assert.Equal("Wet", Assert.Single(result).Name);
        }

        [Fact]
        public void Filter_Dry_ExcludesRainyAndMissingData()
        {
            var reports = new List<SavedReport> { Report("Wet", 2000, rain: 5.0), Report("Damp", 2000, rain: 0.2), Report("Unknown", 2000) };

            var result = ReportQuery.Filter(reports, ReportFilter.Dry);

            Assert.Equal("Damp", Assert.Single(result).Name);
        }

        [Fact]
        public void Filter_BornBefore2000()
        {
            var reports = new List<SavedReport> { Report("Old", 1994), Report("New", 2003) };

            var result = ReportQuery.Filter(reports, ReportFilter.BornBefore2000);

            Assert.Equal("Old", Assert.Single(result).Name);
        }

        [Fact]
        public void Filter_WithSpacePicture()
        {
            var reports = new List<SavedReport> { Report("Space", 2003, spaceImage: "https://img/x.jpg"), Report("None", 1994) };

            var result = ReportQuery.Filter(reports, ReportFilter.WithSpacePicture);

            Assert.Equal("Space", Assert.Single(result).Name);
        }

        [Fact]
        public void Filter_All_ReturnsEverything()
        {
            var reports = new List<SavedReport> { Report("A", 2000), Report("B", 2001) };

            Assert.Equal(2, ReportQuery.Filter(reports, ReportFilter.All).Count);
        }

        // ---------- search and summary ----------

        [Fact]
        public void Search_MatchesNameOrCityIgnoringCase()
        {
            var reports = new List<SavedReport>
            {
                Report("Priya", 2004, city: "Nairobi, Kenya"),
                Report("Mary", 1994, city: "Dublin, Ireland")
            };

            Assert.Equal("Priya", Assert.Single(ReportQuery.Search(reports, "nairobi")).Name);
            Assert.Equal("Mary", Assert.Single(ReportQuery.Search(reports, "MARY")).Name);
        }

        [Fact]
        public void Search_EmptyText_ReturnsEverything()
        {
            var reports = new List<SavedReport> { Report("A", 2000), Report("B", 2001) };

            Assert.Equal(2, ReportQuery.Search(reports, "  ").Count);
        }

        [Fact]
        public void WarmestAndColdest_IgnoreMissingTemperatures()
        {
            var reports = new List<SavedReport> { Report("Cold", 2000, 10.2), Report("None", 2000), Report("Hot", 2000, 24.1) };

            Assert.Equal("Hot", ReportQuery.Warmest(reports)!.Name);
            Assert.Equal("Cold", ReportQuery.Coldest(reports)!.Name);
        }

        [Fact]
        public void Warmest_NoTemperatures_ReturnsNull()
        {
            var reports = new List<SavedReport> { Report("None", 2000) };

            Assert.Null(ReportQuery.Warmest(reports));
        }
    }
}