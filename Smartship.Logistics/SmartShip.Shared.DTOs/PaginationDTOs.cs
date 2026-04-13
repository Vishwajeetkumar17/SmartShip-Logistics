namespace SmartShip.Shared.DTOs;

/// <summary>
/// Represents pagination request parameters
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 5;
}

/// <summary>
/// Generic paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
/// <summary>
/// Represents PaginatedResponse.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Collection of items for the current page
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }
}


