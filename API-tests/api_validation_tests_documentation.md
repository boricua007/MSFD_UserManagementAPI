# API Validation Tests Documentation

This document explains the purpose and expected results for `api_validation_tests.http`.

## 🎯 Primary Purpose
Test the **3 reported bugs** from the TechHive Solutions scenario:

### 1. Users being added without proper validation ✅ FIXED
- **Test 5**: Empty/missing fields → Should now return validation errors
- **Test 6**: Invalid email format → Should now reject bad emails  
- **Test 7**: Null/empty body → Should now handle gracefully instead of crashing

### 2. Errors when retrieving non-existent users ✅ READY FOR STEP 2
- **Test 3**: Get invalid user ID (999) → Should return proper 404
- **Test 9**: Update non-existent user → Should return proper 404  
- **Test 11**: Delete non-existent user → Should return proper 404

### 3. API crashes due to unhandled exceptions ✅ READY FOR STEP 2
- **Test 7**: Previously would crash with null body
- **Tests with invalid IDs**: Previously could cause crashes

## ✅ Positive Tests (Should always work)
- **Test 1**: Get all users → Returns 200 with user list
- **Test 2**: Get valid user → Returns 200 with user details
- **Test 4**: Create valid user → Returns 201 with new user
- **Test 8**: Update valid user → Returns 200 with updated user
- **Test 10**: Delete user → Returns 204 (No Content)

## 📋 Before vs After STEP 1 Fixes

| Test | Before (Buggy) | After (Fixed) |
|------|----------------|---------------|
| Test 5 (empty fields) | ❌ Accepts invalid data | ✅ Returns 400 validation error |
| Test 6 (bad email) | ❌ Accepts "not-an-email" | ✅ Returns 400 validation error |
| Test 7 (null body) | ❌ Crashes with null reference | ✅ Returns 400 "data cannot be null" |

## 🚀 How to Run Tests

1. **Start the API**: Run `dotnet run` in terminal
2. **Open**: `api_validation_tests.http` in VS Code
3. **Execute**: Click "Send Request" above each test
4. **Verify**: Check that responses match expected results below

## 📝 Expected Results

### Validation Tests (STEP 1 Fixes)
- **Test 5**: HTTP 400 with ModelState validation errors
- **Test 6**: HTTP 400 with email validation error
- **Test 7**: HTTP 400 with "User data cannot be null" message

### Error Handling Tests (For STEP 2)
- **Test 3**: HTTP 404 with "User with ID 999 not found"
- **Test 9**: HTTP 404 with "User with ID 999 not found"
- **Test 11**: HTTP 404 with "User with ID 999 not found"

### Success Tests
- **Test 1**: HTTP 200 with array of users
- **Test 2**: HTTP 200 with single user object
- **Test 4**: HTTP 201 with created user object
- **Test 8**: HTTP 200 with updated user object
- **Test 10**: HTTP 204 (empty response)

## 🔧 Debugging Issues

If tests don't return expected results:
1. Check that API is running on http://localhost:5098
2. Verify STEP 1 validation fixes are implemented
3. Check console output for any errors
4. Restart API if needed: Stop with Ctrl+C, then `dotnet run`