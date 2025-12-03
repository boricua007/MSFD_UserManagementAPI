using Microsoft.AspNetCore.Mvc;
using MSFD_UserManagementAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MSFD_UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    // In-memory storage for demonstration purposes
      private static readonly List<User> _users = new()
    {
        new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "555-0101" },
        new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "555-0102" },
        new User { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", PhoneNumber = "555-0103" }
    };
    
    private static int _nextId = 4;

    // Get all users
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<User>> GetUsers([FromQuery] bool? isActive = null)
    {
        var users = _users.AsEnumerable();
        
        if (isActive.HasValue)
        {
            users = users.Where(u => u.IsActive == isActive.Value);
        }
        
        return Ok(users);
    }

    // Get a user by ID
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }
        
        return Ok(user);
    }

    // Create a new user
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<User> CreateUser([FromBody] CreateUserDto userDto)
    {
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

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // Update a user
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<User> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
    {
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

        return Ok(user);
    }


    // Delete a user
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        _users.Remove(user);
        return NoContent();
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