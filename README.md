# MSFD User Management API

.NET 9.0 Web API demonstrating RESTful CRUD operations, OpenAPI/Swagger documentation, and API design patterns for the Microsoft Full Stack Developer certification.

## Features

✅ **Complete CRUD Operations** - Create, Read, Update, and Delete users with validation  
✅ **OpenAPI/Swagger Integration** - Interactive API documentation and testing  
✅ **Advanced Data Validation** - Input validation with detailed error responses and sanitization  
✅ **Performance Optimization** - Pagination, search, sorting, and memory caching  
✅ **Error Handling** - Comprehensive exception handling with proper HTTP status codes  
✅ **RESTful Design** - Standard HTTP methods and status codes  
✅ **In-Memory Storage** - Simple data persistence for demonstration  

**Tech Stack:** .NET 9.0 • C# • ASP.NET Core • Swagger/OpenAPI • Memory Caching

## API Endpoints

| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| GET | `/api/users` | Get all users (with pagination, search, sort) | 200 |
| GET | `/api/users/{id}` | Get user by ID | 200, 404 |
| POST | `/api/users` | Create new user | 201, 400 |
| PUT | `/api/users/{id}` | Update existing user | 200, 400, 404 |
| DELETE | `/api/users/{id}` | Delete user | 204, 404 |

### Query Parameters for GET /api/users
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10, max: 100)
- `search` - Search in firstName, lastName, email
- `sortBy` - Sort by: firstName, lastName, email, dateCreated
- `sortOrder` - Sort order: asc, desc
- `isActive` - Filter by active status

## Usage Examples

### Get All Users
```http
GET http://localhost:5099/api/users
Accept: application/json
```

### Get Users with Pagination and Search
```http
GET http://localhost:5099/api/users?page=1&pageSize=5&search=john&sortBy=firstName&sortOrder=asc
Accept: application/json
```

### Create New User
```http
POST http://localhost:5099/api/users
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "555-0123"
}
```

### Update User
```http
PUT http://localhost:5099/api/users/1
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "phoneNumber": "555-0123",
  "isActive": true
}
```

## Project Structure

```
MSFD_UserManagementAPI/
├── Controllers/
│   └── UsersController.cs                    # CRUD operations controller with caching
├── Models/
│   ├── User.cs                               # User entity model
│   └── UserQueryParameters.cs                # Query parameters for pagination/search
├── API-tests/                                # API testing suite
│   ├── api_validation_tests.http             # Validation test scenarios
│   ├── api_validation_tests_documentation.md
│   ├── api_performance_tests.http            # Performance test scenarios
│   └── api_performance_tests_documentation.md
├── Program.cs                                # Application configuration with caching
├── request.http                              # Basic HTTP test requests
├── appsettings.json                          # Application settings
├── MSFD_UserManagementAPI.csproj             # Project configuration
└── README.md                                 # This file
```

## Getting Started

1. **Clone or download** the project
2. **Navigate to project directory:** `cd MSFD_UserManagementAPI`
3. **Restore dependencies:** `dotnet restore`
4. **Build the application:** `dotnet build`
5. **Run the application:** `dotnet run`
6. **Access Swagger UI:** http://localhost:5099/swagger
7. **Test the API:**
   - Use the `request.http` file in VS Code with REST Client extension
   - Or use the interactive Swagger UI for testing

## Development Workflow

1. **Start the Application:** `dotnet run`
2. **Test CRUD Operations:**
   - Create users with valid data
   - Retrieve users individually and in lists
   - Update user information
   - Delete users and verify removal
3. **Explore API Documentation:**
   - Open Swagger UI at http://localhost:5099/swagger
   - Review OpenAPI specification
   - Test endpoints directly from the browser

## User Model

```csharp
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe", 
  "email": "john.doe@example.com",
  "phoneNumber": "555-0123",
  "dateCreated": "2024-12-02T10:30:00Z",
  "dateUpdated": null,
  "isActive": true
}
```

## Key Concepts Demonstrated

• **RESTful API Design:** Standard HTTP methods and resource-based URLs  
• **Data Transfer Objects (DTOs):** Separate models for API requests and responses  
• **Input Validation:** Model validation with detailed error messages  
• **OpenAPI Documentation:** Auto-generated API documentation with Swagger  
• **Error Handling:** Appropriate HTTP status codes and error responses  

---

*This Web API application demonstrates RESTful API design, CRUD operations, and OpenAPI documentation - essential skills for back-end software developers building scalable web APIs.*
