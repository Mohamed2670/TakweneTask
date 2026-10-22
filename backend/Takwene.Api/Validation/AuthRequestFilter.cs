using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Takwene.Api.Validation;
public sealed class AuthRequestFilter(IConfiguration configuration) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (!request.IsHttps)
        {
            context.Result = new ObjectResult(new ProblemDetails { Status=400, Title="Authentication requires HTTPS." }) { StatusCode=400 };
            return;
        }
        if (!origins.Contains(request.Headers.Origin.ToString(), StringComparer.Ordinal) || request.Headers["X-CSRF"].ToString() != "1")
        {
            context.Result = new ObjectResult(new ProblemDetails { Status=403, Title="Request origin or CSRF header is invalid." }) { StatusCode=403 };
            return;
        }
        await next();
    }
}
