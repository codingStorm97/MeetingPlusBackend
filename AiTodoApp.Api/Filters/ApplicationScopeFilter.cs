using System.Security.Claims;
using System.Text.RegularExpressions;
using AiTodoApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AiTodoApp.Api.Filters;

public class ApplicationScopeFilter : IAsyncActionFilter
{
    private readonly IApplicationScopeContext _applicationScopeContext;

    public ApplicationScopeFilter(IApplicationScopeContext applicationScopeContext)
    {
        _applicationScopeContext = applicationScopeContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid token. UserId claim missing." });
            return;
        }

        var userName = user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue("preferred_username")
            ?? BuildUserNameFromEmail(user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"));

        if (string.IsNullOrWhiteSpace(userName))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid token. UserName claim missing." });
            return;
        }

        _applicationScopeContext.UserId = userId;
        _applicationScopeContext.UserName = userName;
        _applicationScopeContext.UserFolderPath = Path.Combine("Data", userName);

        await next();
    }

    private static string BuildUserNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var localPart = email.Split('@', StringSplitOptions.RemoveEmptyEntries)[0];
        var safe = Regex.Replace(localPart, "[^a-zA-Z0-9_-]", "_");
        return string.IsNullOrWhiteSpace(safe) ? string.Empty : safe;
    }
}
