/// <summary>
/// Provides backend implementation for AdminControllerTests.
/// </summary>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartShip.AdminService.Controllers;
using SmartShip.AdminService.DTOs;
using SmartShip.AdminService.Services;
using SmartShip.Shared.DTOs;

namespace SmartShip.AdminService.UnitTests;

/// <summary>
    /// Represents the admin controller tests entity or configuration model.
    /// </summary>
    [TestFixture]
/// <summary>
/// Represents AdminControllerTests.
/// </summary>
public class AdminControllerTests
{
    private Mock<IAdminService> _serviceMock = null!;
    private Mock<ILogger<AdminController>> _loggerMock = null!;
    private AdminController _controller = null!;

    /// <summary>
    /// Asynchronously handles the set up process.
    /// </summary>
    [SetUp]
    /// <summary>
    /// Executes the SetUp operation.
    /// </summary>
    public void SetUp()
    {
        _serviceMock = new Mock<IAdminService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_serviceMock.Object, _loggerMock.Object);
        SetUser(_controller, userId: 99);
    }

    /// <summary>
    /// Asynchronously handles the logging demo_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the LoggingDemo_CallsServiceAndReturnsOk operation.
    /// </summary>
    public void LoggingDemo_CallsServiceAndReturnsOk()
    {
        _serviceMock
            .Setup(s => s.LogLevelDemo("99", It.IsAny<string>(), true));

        var result = _controller.LoggingDemo(fail: true);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.LogLevelDemo("99", It.IsAny<string>(), true), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get dashboard_returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetDashboard_ReturnsOk operation.
    /// </summary>
    public async Task GetDashboard_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetDashboardMetricsAsync()).ReturnsAsync(new DashboardMetricsDTO());

        var result = await _controller.GetDashboard();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    /// <summary>
    /// Asynchronously handles the create hub_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the CreateHub_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task CreateHub_CallsServiceAndReturnsOk()
    {
        var dto = new CreateHubDTO { Name = "Delhi Distribution Center", Address = "Plot 456, Sector 28, Delhi, DL 110001" };
        _serviceMock.Setup(s => s.CreateHubAsync(dto)).ReturnsAsync(new HubResponseDTO { HubId = 1, Name = "Delhi Distribution Center", Address = "Plot 456, Sector 28, Delhi, DL 110001" });

        var result = await _controller.CreateHub(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.CreateHubAsync(dto), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the delete hub_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the DeleteHub_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task DeleteHub_CallsServiceAndReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteHubAsync(6)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteHub(6);

        Assert.That(result, Is.TypeOf<OkResult>());
        _serviceMock.Verify(s => s.DeleteHubAsync(6), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the resolve exception_sets shipment id before calling service process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the ResolveException_SetsShipmentIdBeforeCallingService operation.
    /// </summary>
    public async Task ResolveException_SetsShipmentIdBeforeCallingService()
    {
        ResolveExceptionDTO? capturedDto = null;
        _serviceMock
            .Setup(s => s.ResolveExceptionAsync(10, It.IsAny<ResolveExceptionDTO>()))
            .Callback<int, ResolveExceptionDTO>((_, dto) => capturedDto = dto)
            .ReturnsAsync(new ExceptionRecordResponseDTO { ExceptionId = 1, ShipmentId = 10 });

        var result = await _controller.ResolveException(10, new ResolveExceptionDTO { ResolutionNotes = "Resolved by admin" });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        Assert.That(capturedDto, Is.Not.Null);
        Assert.That(capturedDto!.ShipmentId, Is.EqualTo(10));
    }

    /// <summary>
    /// Asynchronously handles the delay shipment_passes reason to service process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the DelayShipment_PassesReasonToService operation.
    /// </summary>
    public async Task DelayShipment_PassesReasonToService()
    {
        _serviceMock
            .Setup(s => s.DelayShipmentAsync(12, "Weather delay"))
            .ReturnsAsync(new ExceptionRecordResponseDTO { ExceptionId = 2, ShipmentId = 12 });

        var result = await _controller.DelayShipment(12, new ShipmentActionReasonDTO { Reason = "Weather delay" });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.DelayShipmentAsync(12, "Weather delay"), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get reports_returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetReports_ReturnsOk operation.
    /// </summary>
    public async Task GetReports_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetReportsOverviewAsync()).ReturnsAsync(new { Total = 3 });

        var result = await _controller.GetReports();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    /// <summary>
    /// Asynchronously handles the get all hubs_when called_returns paginated response process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAllHubs_WhenCalled_ReturnsPaginatedResponse operation.
    /// </summary>
    public async Task GetAllHubs_WhenCalled_ReturnsPaginatedResponse()
    {
        var hubs = Enumerable.Range(1, 8).Select(i => new HubResponseDTO
        {
            HubId = i,
            Name = $"Hub {i}",
            Address = $"Address {i}",
            ContactNumber = "1234567890"
        }).ToList();

        var paginatedResponse = new PaginatedResponse<HubResponseDTO>
        {
            Data = hubs.Take(5).ToList(),
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 8,
            TotalPages = 2,
            HasNextPage = true,
            HasPreviousPage = false
        };

        _serviceMock.Setup(s => s.GetAllHubsAsync(1, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAllHubs(1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<HubResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.Data.Count, Is.EqualTo(5));
        Assert.That(returned.TotalPages, Is.EqualTo(2));
        Assert.That(returned.HasNextPage, Is.True);
    }

    /// <summary>
    /// Asynchronously handles the get all locations_when page number2_returns second page process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAllLocations_WhenPageNumber2_ReturnsSecondPage operation.
    /// </summary>
    public async Task GetAllLocations_WhenPageNumber2_ReturnsSecondPage()
    {
        var locations = Enumerable.Range(6, 5).Select(i => new LocationResponseDTO
        {
            LocationId = i,
            Name = $"Location {i}",
            ZipCode = $"10000{i}"
        }).ToList();

        var paginatedResponse = new PaginatedResponse<LocationResponseDTO>
        {
            Data = locations,
            PageNumber = 2,
            PageSize = 5,
            TotalItems = 10,
            TotalPages = 2,
            HasNextPage = false,
            HasPreviousPage = true
        };

        _serviceMock.Setup(s => s.GetAllLocationsAsync(2, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAllLocations(2, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<LocationResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.PageNumber, Is.EqualTo(2));
        Assert.That(returned.HasNextPage, Is.False);
        Assert.That(returned.HasPreviousPage, Is.True);
    }

    /// <summary>
    /// Asynchronously handles the get exceptions_when called_returns paginated exceptions process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetExceptions_WhenCalled_ReturnsPaginatedExceptions operation.
    /// </summary>
    public async Task GetExceptions_WhenCalled_ReturnsPaginatedExceptions()
    {
        var exceptions = Enumerable.Range(1, 3).Select(i => new ExceptionRecordResponseDTO
        {
            ExceptionId = i,
            ShipmentId = i,
            ExceptionType = "Delay",
            Status = "Active"
        }).ToList();

        var paginatedResponse = new PaginatedResponse<ExceptionRecordResponseDTO>
        {
            Data = exceptions,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 3,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _serviceMock.Setup(s => s.GetActiveExceptionsAsync(1, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetExceptions(1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<ExceptionRecordResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.TotalPages, Is.EqualTo(1));
        Assert.That(returned.HasNextPage, Is.False);
    }

    private static void SetUser(ControllerBase controller, int userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, "ADMIN")
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "UnitTestAuth"))
            }
        };
    }
}


