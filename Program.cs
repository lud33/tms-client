using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // -------------------------
        // session2-Service Provider Validation
        // -------------------------
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        // -------------------------
        // session2-Services
        // -------------------------
        builder.Services.AddSingleton<EnrollmentWorker>();
        builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

        // ======-------------------------
        // session3-services
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();

        // session3-Controllers
        builder.Services.AddControllers();

        // session2-payment options with validation
        // -------------------------
        builder.Services.AddOptions<PaymentOptions>()
            .BindConfiguration("Payments")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // -------------------------
        // session1-API + Auth
        // -------------------------
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // -------------------------
        // Middleware pipeline
        // -------------------------
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Exercise 7: Environment Toggle (Dev vs Prod)
        if (app.Environment.IsDevelopment())
        {
            // Development only
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            // Production only
            app.UseExceptionHandler();
        }

        app.UseStatusCodePages();

        // -------------------------
        // Endpoints
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

        app.MapGet("/api/enrollments/worker-smoke",
            (EnrollmentWorker worker) =>
            {
                worker.ProcessBatch();
                return Results.Ok("processed");
            });

        // session3-Controllers
        app.MapGet("/api/error", () =>
        {
            throw new TmsDatabaseException(
                "DatabaseTest",
                "Simulated database failure for ProblemDetails testing");
        });

        app.MapControllers();

        // -------------------------
        app.Run();
    }
}