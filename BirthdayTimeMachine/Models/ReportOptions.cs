namespace BirthdayTimeMachine.Models
{
    // How saved reports can be sorted
    public enum ReportSortOption
    {
        RecentlySaved,
        OldestBirthday,
        NewestBirthday,
        Warmest,
        Coldest,
        Name
    }

    // Which saved reports to show
    public enum ReportFilter
    {
        All,
        Rainy,
        Dry,
        WithSpacePicture,
        BornBefore2000
    }
}