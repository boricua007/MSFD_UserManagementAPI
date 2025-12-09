using System.Diagnostics;
using System.Text;

namespace MSFD_UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware for logging all incoming requests and outgoing responses for auditing purposes.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate a unique request ID for tracking
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();

            // Log incoming request
            await LogRequest(context, requestId);

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                // Replace the response body stream temporarily
                context.Response.Body = responseBody;

                try
                {
                    // Call the next middleware in the pipeline
                    await _next(context);

                    stopwatch.Stop();

                    // Log outgoing response
                    await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "[{RequestId}] Exception occurred during request processing. Duration: {Duration}ms", 
                        requestId, stopwatch.ElapsedMilliseconds);
                    throw;
                }
                finally
                {
                    // Copy the response back to the original stream
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }

        private async Task LogRequest(HttpContext context, string requestId)
        {
            var request = context.Request;

            // Read the request body
            request.EnableBuffering();
            var requestBody = string.Empty;

            if (request.ContentLength > 0)
            {
                using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // Reset the stream position
                }
            }

            _logger.LogInformation(
                "[{RequestId}] Incoming Request: {Method} {Path}{QueryString} | " +
                "ContentType: {ContentType} | ContentLength: {ContentLength} | " +
                "ClientIP: {ClientIP} | UserAgent: {UserAgent}",
                requestId,
                request.Method,
                request.Path,
                request.QueryString,
                request.ContentType ?? "N/A",
                request.ContentLength ?? 0,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                request.Headers["User-Agent"].ToString() ?? "N/A"
            );

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                _logger.LogDebug("[{RequestId}] Request Body: {RequestBody}", requestId, requestBody);
            }
        }

        private async Task LogResponse(HttpContext context, string requestId, long durationMs)
        {
            var response = context.Response;

            // Read the response body
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation(
                "[{RequestId}] Outgoing Response: StatusCode: {StatusCode} | " +
                "ContentType: {ContentType} | ContentLength: {ContentLength} | " +
                "Duration: {Duration}ms",
                requestId,
                response.StatusCode,
                response.ContentType ?? "N/A",
                response.ContentLength ?? responseBody.Length,
                durationMs
            );

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                _logger.LogDebug("[{RequestId}] Response Body: {ResponseBody}", requestId, responseBody);
            }
        }
    }
}
