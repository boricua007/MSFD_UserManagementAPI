namespace MSFD_UserManagementAPI.Middleware
{
    /// <summary>
    /// Extension methods for registering middleware components.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the request and response logging middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }

        /// <summary>
        /// Adds the standardized error handling middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        /// <summary>
        /// Adds the token-based authentication middleware to the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TokenAuthenticationMiddleware>();
        }
    }
}
