using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpotifyTrendsApp.Server.Models;
using System.Net;

namespace SpotifyTrendsApp.Server.Middleware
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var response = exception switch
            {
                ArgumentException argEx => ApiResponse.ErrorResult(argEx.Message, "INVALID_ARGUMENT"),
                UnauthorizedAccessException => ApiResponse.ErrorResult("Access denied", "UNAUTHORIZED"),
                InvalidOperationException invalidOpEx => ApiResponse.ErrorResult(invalidOpEx.Message, "INVALID_OPERATION"),
                HttpRequestException httpEx => ApiResponse.ErrorResult("External service error", "EXTERNAL_SERVICE_ERROR"),
                TaskCanceledException => ApiResponse.ErrorResult("Request timeout", "TIMEOUT"),
                _ => ApiResponse.ErrorResult("An internal server error occurred", "INTERNAL_ERROR")
            };

            var statusCode = exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                InvalidOperationException => HttpStatusCode.BadRequest,
                HttpRequestException => HttpStatusCode.BadGateway,
                TaskCanceledException => HttpStatusCode.RequestTimeout,
                _ => HttpStatusCode.InternalServerError
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
