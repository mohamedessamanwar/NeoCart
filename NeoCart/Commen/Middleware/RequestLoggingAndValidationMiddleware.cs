using NeoCart.Commen.ResponseViewModel;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public sealed class RequestLoggingAndValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingAndValidationMiddleware> _logger;
    private const int MaxLoggedBytes = 1024; // 1KB for simplicity

    public RequestLoggingAndValidationMiddleware(RequestDelegate next, ILogger<RequestLoggingAndValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, IEnumerable<IRequestValidator> validators)
    {
        var start = DateTime.UtcNow;
        var traceId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var ip = GetClientIp(context);
        var userId = GetUserId(context.User);

        // Read request data
        var queryString = GetQueryString(context.Request);
        var bodyText = await ReadRequestBody(context.Request);

        // Run validation
        //var validationErrors = await RunValidators(validators, context, queryString, bodyText);

        //if (validationErrors.Any())
        //{
        //    LogValidationFailure(traceId, method, path, userId, ip, validationErrors, queryString, bodyText);
        //    await WriteValidationErrorResponse(context, traceId, validationErrors);
        //    return;
        //}

        // Log successful request
        LogRequest(traceId, method, path, userId, ip, queryString, bodyText);

        // Continue pipeline
        await _next(context);

        // Log response
        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        LogResponse(traceId, method, path, context.Response.StatusCode, elapsed);
    }
    private static string GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    private static string? GetUserId(ClaimsPrincipal user) =>
        user?.Identity?.IsAuthenticated == true
            ? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.Identity?.Name
            : null;
    private static string GetQueryString(HttpRequest request) =>
        request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
    private async Task<string?> ReadRequestBody(HttpRequest request)
    {
        if (!HasBody(request)) return null;

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return TruncateIfNeeded(body);
    }
    private static bool HasBody(HttpRequest request) =>
        request.Method is "POST" or "PUT" or "PATCH";
    private string? TruncateIfNeeded(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return text.Length > MaxLoggedBytes ? text[..MaxLoggedBytes] + "..." : text;
    }
    //private async Task<List<ValidationError>> RunValidators(IEnumerable<IRequestValidator> validators, 
    //    HttpContext context, string queryString, string? bodyText)
    //{
    //    var errors = new List<ValidationError>();
    //    var queryDict = context.Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

    //    foreach (var validator in validators)
    //    {
    //        try
    //        {
    //            var result = await validator.ValidateAsync(context, queryDict, bodyText);
    //            if (result?.Any() == true) errors.AddRange(result);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Validator {Validator} failed", validator.GetType().Name);
    //            errors.Add(new ValidationError("validator_error", $"Internal validation error"));
    //        }
    //    }

    //    return errors;
    //}

    private void LogRequest(string traceId, string method, string path, string? userId, string ip, string queryString, string? bodyText) =>
        _logger.LogInformation("REQ {TraceId} {Method} {Path} by {User} from {IP} Query={Query} Body={Body}",
            traceId, method, path, userId ?? "anonymous", ip, queryString, bodyText ?? "empty");

    private void LogValidationFailure(string traceId, string method, string path, string? userId, string ip,
        List<ValidationError> errors, string queryString, string? bodyText) =>
        _logger.LogWarning("REQ {TraceId} {Method} {Path} by {User} from {IP} VALIDATION_FAILED Errors={@Errors} Query={Query} Body={Body}",
            traceId, method, path, userId ?? "anonymous", ip, errors, queryString, bodyText ?? "empty");

    private void LogResponse(string traceId, string method, string path, int statusCode, double elapsedMs) =>
        _logger.LogInformation("RES {TraceId} {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
            traceId, method, path, statusCode, Math.Round(elapsedMs, 1));

    private static async Task WriteValidationErrorResponse(HttpContext context, string traceId, List<ValidationError> errors)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = GetErrorDetails(errors);
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>(
            data: null,
            message: message,
            status: false
        );

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static (HttpStatusCode statusCode, string message) GetErrorDetails(List<ValidationError> errors)
    {
        return (HttpStatusCode.BadRequest, "Validation failed");
    }
}

public static class RequestLoggingAndValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLoggingAndValidation(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingAndValidationMiddleware>();
}

public record ValidationError(string Code, string Message);
public interface IRequestValidator
{
    Task<IEnumerable<ValidationError>?> ValidateAsync(HttpContext context, Dictionary<string, string> queryParameters, string? bodyText);
}
