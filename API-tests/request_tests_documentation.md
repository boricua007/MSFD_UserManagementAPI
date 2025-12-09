# User Management API - Request Tests Documentation

## Overview
This document describes the HTTP request tests defined in `request.http` for the User Management API. These tests cover all CRUD operations and various edge cases.

## Setup
- **Base URL Variable**: `@MSFD_UserManagementAPI_HostAddress = http://localhost:5098`
- **Authentication**: Add authentication headers as needed (see AUTHENTICATION.md)
- **Tool Support**: Compatible with VS Code REST Client extension or similar tools

## Test Categories

### 1. GET Requests - Retrieve Users

#### Test 1.1: Get All Users
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users
```
- **Purpose**: Retrieve all users from the system
- **Expected Response**: 200 OK with array of user objects
- **Authentication**: Required

#### Test 1.2: Get Active Users Only
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users?isActive=true
```
- **Purpose**: Filter users by active status
- **Expected Response**: 200 OK with filtered user array
- **Query Parameter**: `isActive=true`
- **Authentication**: Required

#### Test 1.3: Get Inactive Users Only
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users?isActive=false
```
- **Purpose**: Retrieve only deactivated users
- **Expected Response**: 200 OK with filtered user array
- **Query Parameter**: `isActive=false`
- **Authentication**: Required

#### Test 1.4: Get User by ID (Valid)
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users/1
```
- **Purpose**: Retrieve a specific user by ID
- **Expected Response**: 200 OK with single user object
- **Authentication**: Required

#### Test 1.5: Get User by ID (Non-Existent)
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users/999
```
- **Purpose**: Test error handling for non-existent user
- **Expected Response**: 404 Not Found
- **Authentication**: Required

### 2. POST Requests - Create Users

#### Test 2.1: Create New User (Valid Data)
```http
POST {{MSFD_UserManagementAPI_HostAddress}}/api/users
Content-Type: application/json

{
  "firstName": "Alice",
  "lastName": "Wilson",
  "email": "alice.wilson@example.com",
  "phoneNumber": "555-0104"
}
```
- **Purpose**: Create a new user with valid data
- **Expected Response**: 201 Created with user object and Location header
- **Validation**: All fields meet requirements
- **Authentication**: Required

#### Test 2.2: Create User with Duplicate Email
```http
POST {{MSFD_UserManagementAPI_HostAddress}}/api/users
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "email": "john.doe@example.com",
  "phoneNumber": "555-9999"
}
```
- **Purpose**: Test duplicate email validation
- **Expected Response**: 400 Bad Request or 409 Conflict
- **Validation**: Email uniqueness constraint
- **Authentication**: Required

#### Test 2.3: Create User with Invalid Data
```http
POST {{MSFD_UserManagementAPI_HostAddress}}/api/users
Content-Type: application/json

{
  "firstName": "A",
  "lastName": "",
  "email": "invalid-email",
  "phoneNumber": "not-a-phone"
}
```
- **Purpose**: Test validation rules
- **Expected Response**: 400 Bad Request with validation errors
- **Invalid Fields**:
  - `firstName`: Too short (minimum 2 characters)
  - `lastName`: Empty/required
  - `email`: Invalid format
  - `phoneNumber`: Invalid format
- **Authentication**: Required

### 3. PUT Requests - Update Users

#### Test 3.1: Update Existing User (Valid Data)
```http
PUT {{MSFD_UserManagementAPI_HostAddress}}/api/users/2
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith-Updated",
  "email": "jane.smith.updated@example.com",
  "phoneNumber": "555-0199",
  "isActive": true
}
```
- **Purpose**: Update user information with valid data
- **Expected Response**: 200 OK or 204 No Content with updated user
- **Authentication**: Required

#### Test 3.2: Update User with Duplicate Email
```http
PUT {{MSFD_UserManagementAPI_HostAddress}}/api/users/2
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "john.doe@example.com",
  "phoneNumber": "555-0102",
  "isActive": true
}
```
- **Purpose**: Test email uniqueness on update
- **Expected Response**: 400 Bad Request or 409 Conflict
- **Validation**: Cannot update to existing email
- **Authentication**: Required

#### Test 3.3: Update Non-Existent User
```http
PUT {{MSFD_UserManagementAPI_HostAddress}}/api/users/999
Content-Type: application/json

{
  "firstName": "Non",
  "lastName": "Existing",
  "email": "nonexisting@example.com",
  "phoneNumber": "555-0000",
  "isActive": true
}
```
- **Purpose**: Test error handling for updates to non-existent users
- **Expected Response**: 404 Not Found
- **Authentication**: Required

#### Test 3.4: Deactivate User
```http
PUT {{MSFD_UserManagementAPI_HostAddress}}/api/users/3
Content-Type: application/json

{
  "firstName": "Bob",
  "lastName": "Johnson",
  "email": "bob.johnson@example.com",
  "phoneNumber": "555-0103",
  "isActive": false
}
```
- **Purpose**: Test user deactivation (soft delete)
- **Expected Response**: 200 OK or 204 No Content
- **Note**: Sets `isActive=false` instead of physical deletion
- **Authentication**: Required

### 4. DELETE Requests - Remove Users

#### Test 4.1: Delete Existing User
```http
DELETE {{MSFD_UserManagementAPI_HostAddress}}/api/users/3
```
- **Purpose**: Delete a user from the system
- **Expected Response**: 204 No Content or 200 OK
- **Authentication**: Required

#### Test 4.2: Delete Non-Existent User
```http
DELETE {{MSFD_UserManagementAPI_HostAddress}}/api/users/999
```
- **Purpose**: Test error handling for deleting non-existent users
- **Expected Response**: 404 Not Found
- **Authentication**: Required

#### Test 4.3: Verify Deletion
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users/3
```
- **Purpose**: Confirm user was deleted
- **Expected Response**: 404 Not Found
- **Note**: Run after Test 4.1 to verify deletion
- **Authentication**: Required

## Running the Tests

### Prerequisites
1. Start the User Management API
2. Ensure the API is running on `http://localhost:5098` (or update the base URL variable)
3. Have valid authentication tokens (see AUTHENTICATION.md)

### Execution Order
Tests are designed to be run independently, but some tests may depend on existing data:
- Create tests assume certain emails don't exist
- Update tests assume certain user IDs exist
- Delete tests modify the database

### With Authentication
Add authentication header to each request:
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users
Authorization: Bearer dev-token-12345
```

Or use the X-API-Token header:
```http
GET {{MSFD_UserManagementAPI_HostAddress}}/api/users
X-API-Token: dev-token-12345
```

## Expected Behaviors

### Success Responses
- **200 OK**: Successful GET or PUT with response body
- **201 Created**: Successful POST with new resource
- **204 No Content**: Successful DELETE or PUT without response body

### Error Responses
- **400 Bad Request**: Validation errors or invalid data
- **401 Unauthorized**: Missing or invalid authentication token
- **404 Not Found**: Resource doesn't exist
- **409 Conflict**: Duplicate email or constraint violation
- **500 Internal Server Error**: Server-side error

### Response Format
All responses follow the standardized format:
- Success: User object or array
- Error: Standardized error object with `errorId`, `statusCode`, `message`, `timestamp`, and `path`

## Test Coverage
These tests cover:
- ✅ All CRUD operations (Create, Read, Update, Delete)
- ✅ Query parameter filtering
- ✅ Validation rules
- ✅ Duplicate detection
- ✅ Error handling
- ✅ Non-existent resource handling
- ✅ Active/Inactive status management

## Notes
- Tests use in-memory data that may be reset when the API restarts
- IDs may change between runs if using in-memory storage
- Adjust user IDs and emails based on your current data state
- Review validation rules in the User model before running validation tests
