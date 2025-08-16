using NeoCart.Commen.ResponseViewModel;
using System.Net;
using System.Text.Json;

namespace NeoCart.Common.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = GetErrorDetails(exception);
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponse<object>(
                data: null,
                message: message,
                status: false
            );

            //response.Errors.Add(exception.Message);

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static (HttpStatusCode statusCode, string message) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                InvalidOperationException => (HttpStatusCode.Conflict, "Operation not allowed"),
                _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
            };
        }
    }


}