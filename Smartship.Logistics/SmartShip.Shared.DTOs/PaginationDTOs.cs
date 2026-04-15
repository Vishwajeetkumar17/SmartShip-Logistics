namespace SmartShip.Shared.DTOs;

/// <summary>
/// Domain model for pagination request.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page Number value.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page Size value.
    /// </summary>
    public int PageSize { get; set; } = 5;
}

/// <summary>
/// Domain model for paginated response.
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
/// <summary>
/// Domain model for paginated response.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Processes new.
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Page Number value.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page Size value.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total Items value.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total Pages value.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether s next page.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Indicates whether s previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }
}


