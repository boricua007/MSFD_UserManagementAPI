# Token-Based Authentication

## Overview
The User Management API uses token-based authentication to secure endpoints. Tokens must be included in API requests to access protected resources.

## Using Authentication Tokens

### Available Test Tokens
The following tokens are configured in `appsettings.json` for development and testing:

1. **Admin Token**
   - Token: `dev-token-12345`
   - User: Developer User (user-001)
   - Role: Admin

2. **User Token**
   - Token: `test-token-67890`
   - User: Test User (user-002)
   - Role: User

### Including Tokens in Requests

#### Option 1: Authorization Header (Recommended)
```
Authorization: Bearer dev-token-12345
```

#### Option 2: X-API-Token Header
```
X-API-Token: dev-token-12345
```

## Testing Authentication

### Using cURL
```bash
# Without authentication (will fail)
curl -X GET http://localhost:5000/api/users

# With authentication (Authorization header)
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer dev-token-12345"

# With authentication (X-API-Token header)
curl -X GET http://localhost:5000/api/users \
  -H "X-API-Token: dev-token-12345"
```

### Using request.http or API Testing Tools
```http
### Get users (authenticated)
GET http://localhost:5000/api/users
Authorization: Bearer dev-token-12345

### Get auth info (public endpoint)
GET http://localhost:5000/api/auth/info

### Validate token
GET http://localhost:5000/api/auth/validate
Authorization: Bearer dev-token-12345
```

## Public Endpoints (No Authentication Required)
The following endpoints are publicly accessible:
- `/swagger` - Swagger UI
- `/api/auth/info` - Get authentication information
- `/api/auth/login` - Login endpoint (if implemented)
- `/api/auth/token` - Token generation endpoint (if implemented)

## Protected Endpoints (Authentication Required)
All other API endpoints require a valid authentication token:
- `/api/users` - User management endpoints
- `/api/auth/validate` - Token validation endpoint
- All other API resources

## Error Responses

### 401 Unauthorized - Missing Token
```json
{
  "statusCode": 401,
  "message": "Authentication token is required.",
  "timestamp": "2025-12-09T10:30:00Z",
  "path": "/api/users"
}
```

### 401 Unauthorized - Invalid Token
```json
{
  "statusCode": 401,
  "message": "Invalid or expired token.",
  "timestamp": "2025-12-09T10:30:00Z",
  "path": "/api/users"
}
```

## Production Configuration
For production environments:
1. Generate secure, unique tokens
2. Set expiration dates for tokens
3. Store tokens securely (not in source code)
4. Use environment variables or secure configuration providers
5. Consider implementing JWT (JSON Web Tokens) for enhanced security
6. Implement token refresh mechanisms

## Configuration
Edit `appsettings.json` to add or modify valid tokens:

```json
{
  "Authentication": {
    "ValidTokens": [
      {
        "Token": "your-secure-token",
        "UserId": "user-id",
        "UserName": "User Name",
        "Role": "Admin",
        "ExpiresAt": "2025-12-31T23:59:59Z"
      }
    ]
  }
}
```
