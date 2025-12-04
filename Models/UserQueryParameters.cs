using System.ComponentModel.DataAnnotations;

namespace MSFD_UserManagementAPI.Models;

public class UserQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    [RegularExpression("^(firstName|lastName|email|dateCreated)$", 
        ErrorMessage = "SortBy must be one of: firstName, lastName, email, dateCreated")]
    public string SortBy { get; set; } = "dateCreated";

    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortOrder must be 'asc' or 'desc'")]
    public string SortOrder { get; set; } = "desc";
}

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}