using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Takwene.Api.Services;
namespace Takwene.Api.Middleware;
public sealed class ApiExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        ProblemDetails detail;
        if (exception is ApiException api)
            detail = api.Errors is null ? new ProblemDetails { Status=api.StatusCode, Title=api.Message } : new ValidationProblemDetails(api.Errors) { Status=api.StatusCode, Title=api.Message };
        else if (exception is DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } })
            detail = new ProblemDetails { Status=409, Title="A record with the same unique identifier already exists." };
        else if (exception is DbUpdateException { InnerException: SqlException { Number: 547 } })
            detail = new ProblemDetails { Status=400, Title="The request violates a relationship or data constraint." };
        else return false;
        context.Response.StatusCode=detail.Status!.Value;
        detail.Extensions["traceId"]=context.TraceIdentifier;
        return await problems.TryWriteAsync(new ProblemDetailsContext { HttpContext=context, ProblemDetails=detail, Exception=exception });
    }
}
