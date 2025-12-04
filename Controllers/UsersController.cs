using Microsoft.AspNetCore.Mvc;
using MSFD_UserManagementAPI.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;

namespace MSFD_UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMemoryCache _cache;
    // In-memory storage for demonstration purposes
      private static readonly List<User> _users = new()
    {
        new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "555-0101" },
        new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "555-0102" },
        new User { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", PhoneNumber = "555-0103" }
    };
    
    private static int _nextId = 4;

    public UsersController(IMemoryCache cache)
    {
        _cache = cache;
    }

    // Helper method to invalidate user cache
    private void InvalidateUserCache()
    {
        // Remove all cached user queries (this is a simple approach)
        // In a real application, you might want to use cache tags or more sophisticated cache invalidation
        var cacheKeys = new List<string>();
        
        // Since we can't enumerate MemoryCache easily, we'll clear by pattern matching
        // For demo purposes, this simple approach will work
        if (_cache is MemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is object coherentState)
            {
                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                {
                    var keysToRemove = new List<object>();
                    foreach (System.Collections.DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString()?.StartsWith("users_") == true)
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }
                    
                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                    }
                }
            }
        }
    }

    // Get all users with pagination, search, and sorting
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<PagedResult<User>> GetUsers([FromQuery] UserQueryParameters parameters)
    {
        try
        {
            // Validate parameters
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_users == null)
            {
                Console.WriteLine("Error: Users collection is null");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error: Users collection not initialized.");
            }

            // Create cache key for this query
            var cacheKey = $"users_{parameters.Page}_{parameters.PageSize}_{parameters.Search}_{parameters.IsActive}_{parameters.SortBy}_{parameters.SortOrder}";
            
            // Check cache first
            if (_cache.TryGetValue(cacheKey, out PagedResult<User>? cachedResult) && cachedResult != null)
            {
                return Ok(cachedResult);
            }

            // Start with the base query
            IQueryable<User> query = _users.AsQueryable();

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var searchTerm = parameters.Search.ToLowerInvariant().Trim();
                query = query.Where(u => 
                    u.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                    u.LastName.ToLowerInvariant().Contains(searchTerm) ||
                    u.Email.ToLowerInvariant().Contains(searchTerm));
            }

            // Apply active filter
            if (parameters.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == parameters.IsActive.Value);
            }

            // Apply sorting
            query = parameters.SortBy.ToLowerInvariant() switch
            {
                "firstname" => parameters.SortOrder.ToLowerInvariant() == "desc" 
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName),
                "lastname" => parameters.SortOrder.ToLowerInvariant() == "desc"
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName),
                "email" => parameters.SortOrder.ToLowerInvariant() == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                _ => parameters.SortOrder.ToLowerInvariant() == "desc"
                    ? query.OrderByDescending(u => u.DateCreated)
                    : query.OrderBy(u => u.DateCreated)
            };

            // Get total count before pagination
            var totalCount = query.Count();

            // Apply pagination
            var users = query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            // Create paged result
            var result = new PagedResult<User>
            {
                Data = users,
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };

            // Cache the result for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _cache.Set(cacheKey, result, cacheOptions);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving users: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users.");
        }
    }

    // Get a user by ID
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<User> GetUser(int id)
    {
        try
        {
            // Validate ID
            if (id <= 0)
            {
                return BadRequest("User ID must be a positive number.");
            }

            if (_users == null)
            {
                Console.WriteLine("Error: Users collection is null");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error: Users collection not initialized.");
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user with ID {id}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the user.");
        }
    }

    // Create a new user
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<User> CreateUser([FromBody] CreateUserDto userDto)
    {
        try
        {
            // Validate input
            if (userDto == null)
            {
                return BadRequest("User data cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_users == null)
            {
                Console.WriteLine("Error: Users collection is null during create operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error: Users collection not initialized.");
            }

            // Check if email already exists
            if (_users.Any(u => u.Email.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A user with this email already exists.");
            }

            var user = new User
            {
                Id = _nextId++,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                DateCreated = DateTime.UtcNow,
                IsActive = true
            };

            _users.Add(user);

            // Invalidate cache since data has changed
            InvalidateUserCache();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user.");
        }
    }

    // Update a user
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<User> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
    {
        try
        {
            // Validate ID
            if (id <= 0)
            {
                return BadRequest("User ID must be a positive number.");
            }

            // Validate input
            if (userDto == null)
            {
                return BadRequest("User data cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_users == null)
            {
                Console.WriteLine("Error: Users collection is null during update operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error: Users collection not initialized.");
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Check if email already exists for another user
            if (_users.Any(u => u.Id != id && u.Email.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("A user with this email already exists.");
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.IsActive = userDto.IsActive;
            user.DateUpdated = DateTime.UtcNow;

            // Invalidate cache since data has changed
            InvalidateUserCache();

            return Ok(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user with ID {id}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user.");
        }
    }


    // Delete a user
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            // Validate ID
            if (id <= 0)
            {
                return BadRequest("User ID must be a positive number.");
            }

            if (_users == null)
            {
                Console.WriteLine("Error: Users collection is null during delete operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error: Users collection not initialized.");
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            _users.Remove(user);
            
            // Invalidate cache since data has changed
            InvalidateUserCache();
            
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user with ID {id}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user.");
        }
    }
}

// DTOs for API requests
public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
}

public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
}