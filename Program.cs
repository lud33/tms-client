using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Service Provider Validation
// -------------------------
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// -------------------------
// Services
// -------------------------
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// -------------------------
// Options (FIXED CONFIG BINDING)
// MUST match appsettings.json section name: "Payments"
// -------------------------
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// -------------------------
// API + Auth
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

// -------------------------
app.Run();