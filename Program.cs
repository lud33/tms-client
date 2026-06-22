using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // -------------------------
        // Session 2 - Service Provider Validation
        // -------------------------
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        // -------------------------
        // Session 2 - Services
        // -------------------------
        builder.Services.AddSingleton<EnrollmentWorker>();
        builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
        
        // -------------------------
        // Session 5 - EF Core / PostgreSQL
        // -------------------------
        builder.Services.AddDbContext<TmsDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("TmsDatabase"))
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging());

        // -------------------------
        // Session 3 - Services
        // -------------------------
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();

        // module 5 session 2
          builder.Services.AddScoped<StudentQueryService>();
          

        // Session 3 - Controllers
        builder.Services.AddControllers();

        // -------------------------
        // Session 2 - Payment Options Validation
        // -------------------------
        builder.Services.AddOptions<PaymentOptions>()
            .BindConfiguration("Payments")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // -------------------------
        // Session 1 - API + Auth
        // -------------------------
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // -------------------------
        // Session 5 - Apply Migrations Automatically
        // -------------------------
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<TmsDbContext>();

            db.Database.Migrate();

            // FIX: Seed data added (your DB stays intact)
            if (!db.Students.Any())
            {
                var students = new List<TmsApi.Entities.Student>
                {
                    new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
                    new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
                    new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
                    new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
                    new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true }
                };

                var courses = new List<TmsApi.Entities.Course>
                {
                    new() { Code = "CS-101", Title = "Introduction to Computer Science", Capacity = 30 },
                    new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
                    new() { Code = "MAT-101", Title = "Calculus I", Capacity = 40 }
                };

                db.Students.AddRange(students);
                db.Courses.AddRange(courses);
                db.SaveChanges();

                var enrollments = new List<TmsApi.Entities.Enrollment>
                {
                    new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
                    new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
                    new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
                    new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
                };

                db.Enrollments.AddRange(enrollments);
                db.SaveChanges();
            }
        }

        // -------------------------
        // Middleware Pipeline
        // -------------------------
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
       //M5-session 1

       
        // -------------------------
        // Exercise 7: Environment Toggle
        // -------------------------
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.UseStatusCodePages();

        // -------------------------
        // Session 1 Endpoint
        // -------------------------
        app.MapGet("/api/assessments/results", () =>
        {
            return Results.Ok(new
            {
                courseCode = "CS-101",
                studentId = "S-001",
                letterGrade = "A"
            });
        });

        // -------------------------
        // Session 2 Endpoint
        // -------------------------
        app.MapGet("/api/enrollments/worker-smoke",
            (EnrollmentWorker worker) =>
            {
                worker.ProcessBatch();
                return Results.Ok("processed");
            });

        // -------------------------
        // Session 3 Endpoint
        // -------------------------
        app.MapGet("/api/error", () =>
        {
            throw new TmsDatabaseException(
                "DatabaseTest",
                "Simulated database failure for ProblemDetails testing");
        });

        // M5 sesssion 2- page end point



        app.MapGet("/api/students/paged",
    async (int page, StudentQueryService service) =>
{
    return await service.GetStudentsPageAsync(page, 20);
});
          app.MapGet("/api/courses/top",
    async (StudentQueryService service) =>
{
    return await service.GetTopCoursesAsync();
}); 

    // module 5 session 3
   app.MapGet("/api/nplus1-fixed", async (TmsDbContext db) =>
{
    var report = await db.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        })
        .ToListAsync();

    return Results.Ok(report);
});
    app.MapPost("/api/enrollments/archive",
    async (TmsDbContext db) =>
{
    await db.Enrollments
        .Where(e => e.EnrolledAt < DateTime.UtcNow.AddMonths(-1))
        .ExecuteUpdateAsync(s =>
            s.SetProperty(e => e.IsArchived, true));

    return Results.Ok("Archived old enrollments");
});
        // -------------------------
        // Session 3 Controllers
        // -------------------------
        app.MapControllers();

        app.Run();
    }
}