using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult Deferred()
    {
        Console.WriteLine("STEP 1");

        var query = context.Students
            .Where(s => s.GPA >= 3.0m);

        Console.WriteLine("STEP 2");

        var ordered = query.OrderBy(s => s.Name);

        Console.WriteLine("STEP 3");

        var results = ordered.ToList();

        Console.WriteLine("STEP 4");

        return Ok(results);
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TranslationFail()
    {
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}