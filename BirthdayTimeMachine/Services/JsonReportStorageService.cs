using BirthdayTimeMachine.Models;
using BirthdayTimeMachine.Services.Interfaces;
using System.Text.Json;

namespace BirthdayTimeMachine.Services
{
    // Stores saved reports in a JSON file (the brief says a database isn't needed)
    public class JsonReportStorageService : IReportStorageService
    {
        private readonly string _filePath;

        // stops two users writing to the file at the same time
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // the file path is passed in so unit tests can use a temporary file
        public JsonReportStorageService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<SavedReport>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return await ReadFileAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(SavedReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            await _lock.WaitAsync();
            try
            {
                List<SavedReport> reports = await ReadFileAsync();

                // replace it if it was already saved, otherwise add it
                reports.RemoveAll(r => r.Id == report.Id);
                reports.Add(report);

                await WriteFileAsync(reports);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await _lock.WaitAsync();
            try
            {
                List<SavedReport> reports = await ReadFileAsync();
                bool removed = reports.RemoveAll(r => r.Id == id) > 0;

                if (removed)
                {
                    await WriteFileAsync(reports);
                }

                return removed;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<SavedReport>> ReadFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<SavedReport>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<SavedReport>();
                }

                return JsonSerializer.Deserialize<List<SavedReport>>(json, JsonOptions) ?? new List<SavedReport>();
            }
            catch (JsonException)
            {
                // file is corrupt - start with an empty list instead of crashing
                return new List<SavedReport>();
            }
        }

        private async Task WriteFileAsync(List<SavedReport> reports)
        {
            string? folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string json = JsonSerializer.Serialize(reports, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}