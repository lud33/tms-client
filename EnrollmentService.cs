using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string studentId, string courseCode);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly ConcurrentDictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    private static string MakeKey(string studentId, string courseCode)
        => $"{studentId}:{courseCode}";

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        var key = MakeKey(studentId, courseCode);

        var record = _store.GetOrAdd(key, _ =>
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            var newRecord = new EnrollmentRecord(
                id,
                studentId,
                courseCode,
                DateTime.UtcNow);

            _logger.LogInformation(
                "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
                studentId,
                courseCode,
                id);

            return newRecord;
        });

        // If it already existed, log duplicate attempt
        if (record.StudentId == studentId && record.CourseCode == courseCode)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId,
                courseCode,
                record.Id);
        }

        return Task.FromResult(record);
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        var record = _store.Values.FirstOrDefault(x => x.Id == id);

        if (record is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }

        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string studentId, string courseCode)
    {
        var key = MakeKey(studentId, courseCode);

        var removed = _store.TryRemove(key, out var record);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted enrollment {EnrollmentId}",
                record!.Id);
        }
        else
        {
            _logger.LogWarning(
                "Delete failed for {StudentId} in {CourseCode} (not found)",
                studentId,
                courseCode);
        }

        return Task.FromResult(removed);
    }
}

