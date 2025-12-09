using System.Net;
using System.Text.Json;

namespace MSFD_UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware for standardized error handling across all API endpoints.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorId = Guid.NewGuid().ToString();
            
            // Log the exception with error ID for tracking
            _logger.LogError(exception, 
                "Error ID: {ErrorId} | An unhandled exception occurred while processing request {Method} {Path}", 
                errorId, 
                context.Request.Method, 
                context.Request.Path);

            // Determine the appropriate HTTP status code
            var statusCode = DetermineStatusCode(exception);
            
            // Create standardized error response
            var errorResponse = new ErrorResponse
            {
                ErrorId = errorId,
                StatusCode = statusCode,
                Message = GetUserFriendlyMessage(exception, statusCode),
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            };

            // Include detailed error information in development environment
            if (_environment.IsDevelopment())
            {
                errorResponse.DetailedMessage = exception.Message;
                errorResponse.StackTrace = exception.StackTrace;
                errorResponse.ExceptionType = exception.GetType().Name;
            }

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Serialize and write the error response
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private int DetermineStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                TimeoutException => (int)HttpStatusCode.RequestTimeout,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private string GetUserFriendlyMessage(Exception exception, int statusCode)
        {
            return statusCode switch
            {
                400 => "The request was invalid. Please check your input and try again.",
                401 => "You are not authorized to access this resource.",
                404 => "The requested resource was not found.",
                408 => "The request timed out. Please try again.",
                500 => "An internal server error occurred. Please contact support if the issue persists.",
                501 => "This feature is not yet implemented.",
                _ => "An error occurred while processing your request."
            };
        }
    }

    /// <summary>
    /// Standardized error response model.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique identifier for tracking this specific error.
        /// </summary>
        public string ErrorId { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// User-friendly error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the error occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The request path that caused the error.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error message (only in development environment).
        /// </summary>
        public string? DetailedMessage { get; set; }

        /// <summary>
        /// Stack trace (only in development environment).
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Exception type (only in development environment).
        /// </summary>
        public string? ExceptionType { get; set; }
    }
}
