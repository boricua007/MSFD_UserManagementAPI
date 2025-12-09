using Microsoft.AspNetCore.Mvc;

namespace MSFD_UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Provides information about available authentication tokens for testing purposes.
        /// This endpoint is public and does not require authentication.
        /// </summary>
        /// <returns>List of available test tokens</returns>
        [HttpGet("info")]
        public IActionResult GetAuthInfo()
        {
            var tokens = _configuration.GetSection("Authentication:ValidTokens")
                .Get<List<Middleware.TokenConfig>>() ?? new List<Middleware.TokenConfig>();

            var tokenInfo = tokens.Select(t => new
            {
                token = t.Token,
                userName = t.UserName,
                role = t.Role,
                expiresAt = t.ExpiresAt,
                instructions = "Include this token in the Authorization header as 'Bearer {token}' or in the X-API-Token header"
            });

            return Ok(new
            {
                message = "Available authentication tokens for testing",
                tokens = tokenInfo,
                examples = new
                {
                    authorizationHeader = "Authorization: Bearer dev-token-12345",
                    apiTokenHeader = "X-API-Token: dev-token-12345"
                }
            });
        }

        /// <summary>
        /// Validates the current authentication token.
        /// Requires authentication.
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    authenticated = true,
                    userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
                    role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                });
            }

            return Unauthorized(new { authenticated = false, message = "Not authenticated" });
        }
    }
}
