using SmartShip.Shared.DTOs;

namespace SmartShip.Shared.Common.Extensions;

/// <summary>
/// Extension methods for pagination operations
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Converts an enumerable collection to a paginated response
    /// </summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    /// <param name="items">The enumerable collection to paginate</param>
    /// <param name="pageNumber">Current page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="totalCount">Total count of items before pagination</param>
    /// <returns>Paginated response object with metadata</returns>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        // Ensure page number doesn't exceed total pages
        if (pageNumber > totalPages && totalCount > 0)
        {
            pageNumber = totalPages;
        }

        return new PaginatedResponse<T>
        {
            Data = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1
        };
    }

    /// <summary>
    /// Applies pagination to an IQueryable collection
    /// </summary>
    /// <typeparam name="T">Type of items in the queryable</typeparam>
    /// <param name="query">The IQueryable collection</param>
    /// <param name="pageNumber">Current page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated queryable collection</returns>
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        // Calculate skip count
        var skip = (pageNumber - 1) * pageSize;

        // Apply skip and take
        return query.Skip(skip).Take(pageSize);
    }
}


