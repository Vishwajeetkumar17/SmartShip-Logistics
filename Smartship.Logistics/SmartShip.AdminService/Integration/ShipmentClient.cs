/// <summary>
/// Provides backend implementation for ShipmentClient.
/// </summary>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Integration;

/// <summary>
/// Represents ShipmentClient.
/// </summary>
public class ShipmentClient : IShipmentClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the shipment client class.
    /// </summary>
    public ShipmentClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Executes the GetAllShipmentsAsync operation.
    /// </summary>
    public async Task<List<ShipmentExternalDto>> GetAllShipmentsAsync()
    {
        const int pageSize = 100;
        var pageNumber = 1;
        var shipments = new List<ShipmentExternalDto>();

        while (true)
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/shipments?pageNumber={pageNumber}&pageSize={pageSize}");
            var response = await SendAsync(request);
            var (pageItems, hasNextPage) = await ReadShipmentsAsync(response.Content);

            if (pageItems.Count == 0)
            {
                break;
            }

            shipments.AddRange(pageItems);

            if (!hasNextPage)
            {
                break;
            }

            pageNumber++;
        }

        return shipments;
    }

    /// <summary>
    /// Executes the GetShipmentByIdAsync operation.
    /// </summary>
    public async Task<ShipmentExternalDto?> GetShipmentByIdAsync(int shipmentId)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/shipments/{shipmentId}");
        var response = await SendAsync(request);

        return await response.Content.ReadFromJsonAsync<ShipmentExternalDto>(JsonOptions);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        var httpContext = _httpContextAccessor.HttpContext;

        // Forward Authorization header
        var authHeader = httpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && AuthenticationHeaderValue.TryParse(authHeader, out var headerValue))
        {
            request.Headers.Authorization = headerValue;
        }

        // ✓ Propagate Correlation ID for distributed tracing
        if (httpContext != null)
        {
            var correlationId = httpContext.TraceIdentifier;
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }
        }

        return request;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("Unable to reach ShipmentService.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException("Requested shipment was not found in ShipmentService.");
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("AdminService is not authorized to call ShipmentService with the provided token.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"ShipmentService request failed with status code {(int)response.StatusCode}. Response: {errorBody}");
        }

        return response;
    }

    private static async Task<(List<ShipmentExternalDto> Items, bool HasNextPage)> ReadShipmentsAsync(HttpContent content)
    {
        var payload = await content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(payload))
        {
            return (new List<ShipmentExternalDto>(), false);
        }

        try
        {
            var paginated = JsonSerializer.Deserialize<PaginatedResponse<ShipmentExternalDto>>(payload, JsonOptions);
            if (paginated?.Data is { Count: > 0 })
            {
                var hasNextPage = paginated.HasNextPage || (paginated.PageNumber < paginated.TotalPages);
                return (paginated.Data, hasNextPage);
            }

            if (paginated?.Data is { Count: 0 })
            {
                return (new List<ShipmentExternalDto>(), false);
            }
        }
        catch (JsonException)
        {
            // Fall through to non-paginated payload parsing.
        }

        var directList = JsonSerializer.Deserialize<List<ShipmentExternalDto>>(payload, JsonOptions);
        return (directList ?? new List<ShipmentExternalDto>(), false);
    }
}


