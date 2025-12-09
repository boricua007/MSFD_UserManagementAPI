using System.Security.Claims;

namespace MSFD_UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware for token-based authentication to secure API endpoints.
    /// </summary>
    public class TokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenAuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        // Paths that don't require authentication
        private readonly HashSet<string> _publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/swagger",
            "/swagger/index.html",
            "/swagger/v1/swagger.json",
            "/api/auth/login",
            "/api/auth/token",
            "/api/auth/info"
        };

        public TokenAuthenticationMiddleware(
            RequestDelegate next, 
            ILogger<TokenAuthenticationMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Skip authentication for public paths
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            // Extract token from Authorization header
            var token = ExtractToken(context);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authentication failed: No token provided for {Path}", path);
                await RespondWithUnauthorized(context, "Authentication token is required.");
                return;
            }

            // Validate the token
            var validationResult = ValidateToken(token);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Authentication failed: {Reason} for {Path}", validationResult.ErrorMessage, path);
                await RespondWithUnauthorized(context, validationResult.ErrorMessage ?? "Authentication failed.");
                return;
            }

            // Add user claims to the context
            context.User = validationResult.Principal!;
            _logger.LogInformation("User authenticated successfully: {UserId} for {Path}", 
                validationResult.UserId, path);

            await _next(context);
        }

        private bool IsPublicPath(string path)
        {
            return _publicPaths.Any(publicPath => 
                path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase));
        }

        private string? ExtractToken(HttpContext context)
        {
            // Check Authorization header (Bearer token)
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // Alternatively, check for X-API-Token header
            var apiTokenHeader = context.Request.Headers["X-API-Token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiTokenHeader))
            {
                return apiTokenHeader;
            }

            return null;
        }

        private TokenValidationResult ValidateToken(string token)
        {
            try
            {
                // Get valid tokens from configuration
                var validTokens = _configuration.GetSection("Authentication:ValidTokens").Get<List<TokenConfig>>() 
                    ?? new List<TokenConfig>();

                // Find matching token
                var tokenConfig = validTokens.FirstOrDefault(t => t.Token == token);

                if (tokenConfig == null)
                {
                    return TokenValidationResult.Failed("Invalid or expired token.");
                }

                // Check if token is expired
                if (tokenConfig.ExpiresAt.HasValue && tokenConfig.ExpiresAt.Value < DateTime.UtcNow)
                {
                    return TokenValidationResult.Failed("Token has expired.");
                }

                // Create claims principal
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, tokenConfig.UserId),
                    new Claim(ClaimTypes.Name, tokenConfig.UserName),
                    new Claim(ClaimTypes.Role, tokenConfig.Role)
                };

                var identity = new ClaimsIdentity(claims, "TokenAuthentication");
                var principal = new ClaimsPrincipal(identity);

                return TokenValidationResult.Success(principal, tokenConfig.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return TokenValidationResult.Failed("Token validation error.");
            }
        }

        private async Task RespondWithUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = 401,
                message = message,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>
    /// Configuration for a valid authentication token.
    /// </summary>
    public class TokenConfig
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Result of token validation.
    /// </summary>
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public ClaimsPrincipal? Principal { get; set; }
        public string? UserId { get; set; }

        public static TokenValidationResult Success(ClaimsPrincipal principal, string userId)
        {
            return new TokenValidationResult
            {
                IsValid = true,
                Principal = principal,
                UserId = userId
            };
        }

        public static TokenValidationResult Failed(string errorMessage)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
