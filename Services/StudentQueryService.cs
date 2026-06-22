using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class StudentQueryService
{
    private readonly TmsDbContext _context;

    public StudentQueryService(TmsDbContext context)
    {
        _context = context;
    }

   // exercise 3 pagination
    public async Task<List<Student>> GetStudentsPageAsync(int page, int pageSize)
    {
        return await _context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

   
    public async Task<List<CourseEnrollmentSummary>> GetTopCoursesAsync()
{
    return await _context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new CourseEnrollmentSummary
        {
            CourseTitle = g.Key,
            EnrollmentCount = g.Count()
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .Take(5)
        .ToListAsync();
}
}