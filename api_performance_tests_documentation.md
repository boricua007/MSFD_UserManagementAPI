# API Performance Tests Documentation

This document explains the purpose and expected results for `api_performance_tests.http`.

## 🎯 Primary Purpose
Test the **performance improvements** implemented in STEP 3:

### ✅ Pagination Features
- **Test 1**: Default pagination (page=1, pageSize=10)
- **Test 2**: Custom page size (5 users per page)
- **Test 3**: Navigation to second page
- **Test 12**: Validation of page size limits (max 100)

### ✅ Search Capabilities
- **Test 4**: Search by first name ("john")
- **Test 5**: Search by email domain ("example.com")
- **Test 14**: Case-insensitive search ("JOHN")
- **Test 15**: Empty search handling

### ✅ Filtering Options
- **Test 6**: Filter by active status
- **Test 10**: Combined filtering with other parameters

### ✅ Sorting Features
- **Test 7**: Sort by firstName (ascending)
- **Test 8**: Sort by email (descending)
- **Test 9**: Sort by dateCreated (newest first)
- **Test 13**: Validation of sort field names

### ✅ Caching Performance
- **Test 11**: Cache effectiveness test (run twice to see caching)
- **Test 10**: Complex queries with caching

### ✅ Parameter Validation
- **Test 12**: Invalid page size (>100) → Returns 400
- **Test 13**: Invalid sort field → Returns 400

## 📋 Performance Improvements Tested

| Issue Fixed | Test Cases | Expected Improvement |
|-------------|------------|---------------------|
| **No Pagination** | Tests 1-3, 12 | Returns only requested page, not all data |
| **No Search** | Tests 4-5, 14-15 | Fast text search across name/email fields |
| **No Sorting** | Tests 7-9, 13 | Flexible sorting by multiple fields |
| **No Caching** | Test 11 | Faster response times for repeated queries |
| **Memory Inefficiency** | All tests | Optimized LINQ queries, less memory usage |

## 🚀 How to Test Performance

1. **Start the API**: Run `dotnet run` in terminal
2. **Open**: `api_performance_tests.http` in VS Code
3. **Execute**: Click "Send Request" above each test
4. **Compare**: Notice response structure differences from old API

## 📝 Expected Response Format

### New Paginated Response Structure:
```json
{
  "data": [
    { "id": 1, "firstName": "John", "lastName": "Doe", ... }
  ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

### Before vs After:
- **Before**: Returns array of all users
- **After**: Returns paginated result with metadata

## 🔧 Performance Benchmarks

**Cache Performance:**
- **First Request**: ~50-100ms (no cache)
- **Cached Request**: ~5-10ms (from cache)
- **Cache Duration**: 5 minutes absolute, 2 minutes sliding

**Memory Usage:**
- **Before**: Loads all users into memory
- **After**: Only loads requested page

**Search Performance:**
- **Case-insensitive**: Optimized string comparisons
- **Multiple fields**: Searches name and email simultaneously

## 🛠️ Troubleshooting

If performance tests fail:
1. Verify API is running on http://localhost:5098
2. Check that UserQueryParameters model exists
3. Ensure memory caching is registered in Program.cs
4. Restart API if caching issues occur