namespace Takwene.Api.Services;
public sealed class ApiException(int statusCode, string title, IDictionary<string, string[]>? errors = null) : Exception(title)
{
    public int StatusCode { get; } = statusCode;
    public IDictionary<string, string[]>? Errors { get; } = errors;
}
