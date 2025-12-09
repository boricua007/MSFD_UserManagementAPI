using MSFD_UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // Add memory caching

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================================================
// Configure the HTTP request pipeline - Middleware Order is Critical!
// ============================================================================
// 1. Error Handling (catches exceptions from all downstream middleware)
// 2. Swagger (Development only - for API documentation)
// 3. Authentication (validates tokens before processing)
// 4. Request/Response Logging (logs after authentication, before controllers)
// 5. Authorization (checks user permissions)
// 6. Controllers (endpoint routing)
// ============================================================================

// Error handling middleware (must be first to catch all exceptions)
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication middleware (validates tokens)
app.UseTokenAuthentication();

// Request/response logging middleware (logs authenticated requests)
app.UseRequestResponseLogging();

// Enable HTTPS redirection in production
// app.UseHttpsRedirection();

// Authorization middleware
app.UseAuthorization();

app.MapControllers();

app.Run();