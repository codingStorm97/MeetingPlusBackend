using AiTodoApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly DatabaseInitializer _databaseInitializer;

    public SetupController(DatabaseInitializer databaseInitializer)
    {
        _databaseInitializer = databaseInitializer;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        await _databaseInitializer.MigrateAndSeedAsync(cancellationToken);
        return Ok(new { message = "Database migrated and seed data applied successfully." });
    }
}
