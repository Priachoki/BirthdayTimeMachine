using BirthdayTimeMachine.Models;

namespace BirthdayTimeMachine.Helpers
{
    // Search, filter and sort logic for saved reports
    public static class ReportQuery
    {
        public static List<SavedReport> Search(IEnumerable<SavedReport> reports, string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return reports.ToList();
            }

            string term = text.Trim();

            return reports
                .Where(r => r.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                         || r.City.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<SavedReport> Filter(IEnumerable<SavedReport> reports, ReportFilter filter)
        {
            return filter switch
            {
                ReportFilter.Rainy => reports.Where(r => r.WasRainy).ToList(),
                ReportFilter.Dry => reports.Where(r => r.Precipitation.HasValue && !r.WasRainy).ToList(),
                ReportFilter.WithSpacePicture => reports.Where(r => !string.IsNullOrEmpty(r.SpaceImageUrl)).ToList(),
                ReportFilter.BornBefore2000 => reports.Where(r => r.BirthDate.Year < 2000).ToList(),
                _ => reports.ToList()
            };
        }

        public static List<SavedReport> Sort(IEnumerable<SavedReport> reports, ReportSortOption option)
        {
            // copy the list so the original isn't changed
            var list = new List<SavedReport>(reports);

            switch (option)
            {
                case ReportSortOption.OldestBirthday:
                    list.Sort();                                   // uses IComparable on SavedReport
                    break;
                case ReportSortOption.NewestBirthday:
                    list.Sort();
                    list.Reverse();
                    break;
                case ReportSortOption.Warmest:
                    list.Sort(new TemperatureComparer(warmestFirst: true));   // IComparer
                    break;
                case ReportSortOption.Coldest:
                    list.Sort(new TemperatureComparer());
                    break;
                case ReportSortOption.Name:
                    list.Sort(new NameComparer());
                    break;
                case ReportSortOption.RecentlySaved:
                    list = list.OrderByDescending(r => r.SavedAt).ToList(); // LINQ
                    break;
            }

            return list;
        }

        public static SavedReport? Warmest(IEnumerable<SavedReport> reports)
        {
            return reports
                .Where(r => r.MaxTemperature.HasValue)
                .OrderByDescending(r => r.MaxTemperature)
                .FirstOrDefault();
        }

        public static SavedReport? Coldest(IEnumerable<SavedReport> reports)
        {
            return reports
                .Where(r => r.MaxTemperature.HasValue)
                .OrderBy(r => r.MaxTemperature)
                .FirstOrDefault();
        }
    }
}