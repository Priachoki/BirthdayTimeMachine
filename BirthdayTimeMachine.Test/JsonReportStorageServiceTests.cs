using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services;

namespace BirthdayTimeMachine.Test
{
    public class JsonReportStorageServiceTests : IDisposable
    {
        private readonly string _folder;
        private readonly string _filePath;

        public JsonReportStorageServiceTests()
        {
            // a fresh temporary folder for every test
            _folder = Path.Combine(Path.GetTempPath(), "BirthdayTimeMachineTests_" + Guid.NewGuid());
            _filePath = Path.Combine(_folder, "Data", "reports.json");
        }

        public void Dispose()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }

        private static SavedReport SampleReport(string name = "Priya")
        {
            return new SavedReport
            {
                Name = name,
                BirthDate = new DateOnly(2004, 1, 1),
                City = "Nairobi, Nairobi County, Kenya",
                MaxTemperature = 24.1,
                SpaceTitle = "Structure in N63A"
            };
        }

        [Fact]
        public async Task GetAllAsync_NoFile_ReturnsEmptyList()
        {
            var storage = new JsonReportStorageService(_filePath);

            List<SavedReport> reports = await storage.GetAllAsync();

            Assert.Empty(reports);
        }

        [Fact]
        public async Task SaveAsync_ThenGetAll_ReturnsSameData()
        {
            // Arrange
            var storage = new JsonReportStorageService(_filePath);
            SavedReport original = SampleReport();

            // Act
            await storage.SaveAsync(original);
            List<SavedReport> reports = await storage.GetAllAsync();

            // Assert
            SavedReport loaded = Assert.Single(reports);
            Assert.Equal(original.Id, loaded.Id);
            Assert.Equal("Priya", loaded.Name);
            Assert.Equal(new DateOnly(2004, 1, 1), loaded.BirthDate);
            Assert.Equal(24.1, loaded.MaxTemperature);
            Assert.Equal("Structure in N63A", loaded.SpaceTitle);
        }

        [Fact]
        public async Task SaveAsync_SameIdTwice_UpdatesInsteadOfDuplicating()
        {
            var storage = new JsonReportStorageService(_filePath);
            SavedReport report = SampleReport();

            await storage.SaveAsync(report);
            report.Name = "Priyanka";
            await storage.SaveAsync(report);

            SavedReport loaded = Assert.Single(await storage.GetAllAsync());
            Assert.Equal("Priyanka", loaded.Name);
        }

        [Fact]
        public async Task DeleteAsync_ExistingReport_RemovesItAndReturnsTrue()
        {
            var storage = new JsonReportStorageService(_filePath);
            SavedReport keep = SampleReport("Keep");
            SavedReport remove = SampleReport("Remove");
            await storage.SaveAsync(keep);
            await storage.SaveAsync(remove);

            bool deleted = await storage.DeleteAsync(remove.Id);

            Assert.True(deleted);
            Assert.Equal("Keep", Assert.Single(await storage.GetAllAsync()).Name);
        }

        [Fact]
        public async Task DeleteAsync_UnknownId_ReturnsFalse()
        {
            var storage = new JsonReportStorageService(_filePath);
            await storage.SaveAsync(SampleReport());

            bool deleted = await storage.DeleteAsync(Guid.NewGuid());

            Assert.False(deleted);
        }

        [Fact]
        public async Task GetAllAsync_CorruptFile_ReturnsEmptyListInsteadOfCrashing()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            await File.WriteAllTextAsync(_filePath, "{ this is not valid json");
            var storage = new JsonReportStorageService(_filePath);

            List<SavedReport> reports = await storage.GetAllAsync();

            Assert.Empty(reports);
        }

        [Fact]
        public async Task SaveAsync_FolderDoesNotExist_CreatesIt()
        {
            var storage = new JsonReportStorageService(_filePath);

            await storage.SaveAsync(SampleReport());

            Assert.True(File.Exists(_filePath));
        }
    }
}