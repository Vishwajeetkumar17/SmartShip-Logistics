/// <summary>
/// Provides backend implementation for ShipmentsControllerTests.
/// </summary>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartShip.ShipmentService.Controllers;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Models;
using SmartShip.ShipmentService.Services;
using SmartShip.Shared.DTOs;

namespace SmartShip.ShipmentService.UnitTests;

/// <summary>
    /// Represents the shipments controller tests entity or configuration model.
    /// </summary>
    [TestFixture]
/// <summary>
/// Represents ShipmentsControllerTests.
/// </summary>
public class ShipmentsControllerTests
{
    private Mock<IShipmentService> _shipmentServiceMock = null!;
    private ShipmentsController _controller = null!;

    /// <summary>
    /// Asynchronously handles the set up process.
    /// </summary>
    [SetUp]
    /// <summary>
    /// Executes the SetUp operation.
    /// </summary>
    public void SetUp()
    {
        _shipmentServiceMock = new Mock<IShipmentService>(MockBehavior.Strict);
        _controller = new ShipmentsController(_shipmentServiceMock.Object);
    }

    /// <summary>
    /// Asynchronously handles the create_when customer claim missing_returns unauthorized process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Create_WhenCustomerClaimMissing_ReturnsUnauthorized operation.
    /// </summary>
    public async Task Create_WhenCustomerClaimMissing_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);

        var dto = BuildCreateShipmentDto();

        var result = await _controller.Create(dto);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _shipmentServiceMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Asynchronously handles the create_when non admin_assigns customer id and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Create_WhenNonAdmin_AssignsCustomerIdAndReturnsOk operation.
    /// </summary>
    public async Task Create_WhenNonAdmin_AssignsCustomerIdAndReturnsOk()
    {
        SetUser(_controller, userId: 22, isAdmin: false);
        _shipmentServiceMock
            .Setup(s => s.CreateShipment(It.IsAny<CreateShipmentDTO>()))
            .ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 101, CustomerId = 22, TrackingNumber = "TRK-001" });

        var dto = BuildCreateShipmentDto();

        var result = await _controller.Create(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        Assert.That(dto.CustomerId, Is.EqualTo(22));
        _shipmentServiceMock.Verify(s => s.CreateShipment(It.Is<CreateShipmentDTO>(x => x.CustomerId == 22)), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get_when shipment not found_returns not found process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Get_WhenShipmentNotFound_ReturnsNotFound operation.
    /// </summary>
    public async Task Get_WhenShipmentNotFound_ReturnsNotFound()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        _shipmentServiceMock.Setup(s => s.GetShipment(777)).ReturnsAsync((ShipmentResponseDTO?)null);

        var result = await _controller.Get(777);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        _shipmentServiceMock.Verify(s => s.GetShipment(777), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get_when non admin accesses another customer_returns forbid process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Get_WhenNonAdminAccessesAnotherCustomer_ReturnsForbid operation.
    /// </summary>
    public async Task Get_WhenNonAdminAccessesAnotherCustomer_ReturnsForbid()
    {
        SetUser(_controller, userId: 5, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(11)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 11, CustomerId = 7 });

        var result = await _controller.Get(11);

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    /// <summary>
    /// Asynchronously handles the get_when non admin accesses own shipment_returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Get_WhenNonAdminAccessesOwnShipment_ReturnsOk operation.
    /// </summary>
    public async Task Get_WhenNonAdminAccessesOwnShipment_ReturnsOk()
    {
        SetUser(_controller, userId: 5, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(11)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 11, CustomerId = 5 });

        var result = await _controller.Get(11);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    /// <summary>
    /// Asynchronously handles the schedule pickup_when shipment missing_returns not found process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the SchedulePickup_WhenShipmentMissing_ReturnsNotFound operation.
    /// </summary>
    public async Task SchedulePickup_WhenShipmentMissing_ReturnsNotFound()
    {
        SetUser(_controller, userId: 5, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(9)).ReturnsAsync((ShipmentResponseDTO?)null);

        var result = await _controller.SchedulePickup(9, new PickupScheduleDTO { PickupDate = DateTime.Now.AddHours(4) });

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    /// <summary>
    /// Asynchronously handles the schedule pickup_when non owner_returns forbid process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the SchedulePickup_WhenNonOwner_ReturnsForbid operation.
    /// </summary>
    public async Task SchedulePickup_WhenNonOwner_ReturnsForbid()
    {
        SetUser(_controller, userId: 1, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(9)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 9, CustomerId = 2 });

        var result = await _controller.SchedulePickup(9, new PickupScheduleDTO { PickupDate = DateTime.Now.AddHours(4) });

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    /// <summary>
    /// Asynchronously handles the schedule pickup_when admin_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the SchedulePickup_WhenAdmin_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task SchedulePickup_WhenAdmin_CallsServiceAndReturnsOk()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var schedule = new PickupScheduleDTO { PickupDate = DateTime.Now.AddHours(4), Notes = "Front desk" };
        _shipmentServiceMock.Setup(s => s.SchedulePickup(9, schedule)).Returns(Task.CompletedTask);

        var result = await _controller.SchedulePickup(9, schedule);

        Assert.That(result, Is.TypeOf<OkResult>());
        _shipmentServiceMock.Verify(s => s.SchedulePickup(9, schedule), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the raise issue_when unauthenticated_returns unauthorized process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the RaiseIssue_WhenUnauthenticated_ReturnsUnauthorized operation.
    /// </summary>
    public async Task RaiseIssue_WhenUnauthenticated_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);

        var result = await _controller.RaiseIssue(1, new ShipmentIssueDTO { IssueType = "Damage", Description = "Damaged package" });

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    /// <summary>
    /// Asynchronously handles the raise issue_when owner_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the RaiseIssue_WhenOwner_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task RaiseIssue_WhenOwner_CallsServiceAndReturnsOk()
    {
        SetUser(_controller, userId: 3, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(88)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 88, CustomerId = 3 });
        _shipmentServiceMock
            .Setup(s => s.RaiseIssueAsync(88, 3, It.IsAny<ShipmentIssueDTO>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.RaiseIssue(88, new ShipmentIssueDTO { IssueType = "Damage", Description = "Package wet" });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _shipmentServiceMock.Verify(s => s.RaiseIssueAsync(88, 3, It.IsAny<ShipmentIssueDTO>()), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the pickup_when called_uses picked up status process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the Pickup_WhenCalled_UsesPickedUpStatus operation.
    /// </summary>
    public async Task Pickup_WhenCalled_UsesPickedUpStatus()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        _shipmentServiceMock.Setup(s => s.UpdateStatus(4, ShipmentStatus.PickedUp, "DEL-HUB")).Returns(Task.CompletedTask);

        var result = await _controller.Pickup(4, new ShipmentStatusUpdateDTO { HubLocation = "DEL-HUB" });

        Assert.That(result, Is.TypeOf<OkResult>());
        _shipmentServiceMock.Verify(s => s.UpdateStatus(4, ShipmentStatus.PickedUp, "DEL-HUB"), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get all_when called_returns paginated response process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAll_WhenCalled_ReturnsPaginatedResponse operation.
    /// </summary>
    public async Task GetAll_WhenCalled_ReturnsPaginatedResponse()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var items = Enumerable.Range(1, 12).Select(i => new ShipmentResponseDTO
        {
            ShipmentId = i,
            CustomerId = i % 3 + 1,
            TrackingNumber = $"TRK-{i:D4}",
            Status = ShipmentStatus.Booked
        }).ToList();

        var paginatedResponse = new PaginatedResponse<ShipmentResponseDTO>
        {
            Data = items.Take(5).ToList(),
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = false
        };

        _shipmentServiceMock.Setup(s => s.GetShipments(1, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAll(1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<ShipmentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.Data.Count, Is.EqualTo(5));
        Assert.That(returned.TotalItems, Is.EqualTo(12));
        Assert.That(returned.TotalPages, Is.EqualTo(3));
        Assert.That(returned.HasNextPage, Is.True);
        Assert.That(returned.HasPreviousPage, Is.False);
    }

    /// <summary>
    /// Asynchronously handles the get all_when page number2_returns second page process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAll_WhenPageNumber2_ReturnsSecondPage operation.
    /// </summary>
    public async Task GetAll_WhenPageNumber2_ReturnsSecondPage()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var items = Enumerable.Range(6, 5).Select(i => new ShipmentResponseDTO
        {
            ShipmentId = i,
            CustomerId = i % 3 + 1,
            TrackingNumber = $"TRK-{i:D4}",
            Status = ShipmentStatus.InTransit
        }).ToList();

        var paginatedResponse = new PaginatedResponse<ShipmentResponseDTO>
        {
            Data = items,
            PageNumber = 2,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = true
        };

        _shipmentServiceMock.Setup(s => s.GetShipments(2, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAll(2, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<ShipmentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.PageNumber, Is.EqualTo(2));
        Assert.That(returned.Data.Count, Is.EqualTo(5));
        Assert.That(returned.HasNextPage, Is.True);
        Assert.That(returned.HasPreviousPage, Is.True);
    }

    /// <summary>
    /// Asynchronously handles the get all_when last page_verifies pagination metadata process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAll_WhenLastPage_VerifiesPaginationMetadata operation.
    /// </summary>
    public async Task GetAll_WhenLastPage_VerifiesPaginationMetadata()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var items = Enumerable.Range(11, 2).Select(i => new ShipmentResponseDTO
        {
            ShipmentId = i,
            CustomerId = i % 3 + 1,
            TrackingNumber = $"TRK-{i:D4}",
            Status = ShipmentStatus.Delivered
        }).ToList();

        var paginatedResponse = new PaginatedResponse<ShipmentResponseDTO>
        {
            Data = items,
            PageNumber = 3,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = false,
            HasPreviousPage = true
        };

        _shipmentServiceMock.Setup(s => s.GetShipments(3, 5)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAll(3, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<ShipmentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.HasNextPage, Is.False);
        Assert.That(returned.HasPreviousPage, Is.True);
        Assert.That(returned.PageNumber, Is.EqualTo(3));
    }

    /// <summary>
    /// Asynchronously handles the get all_when custom page size_returns smaller pages process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetAll_WhenCustomPageSize_ReturnsSmallerPages operation.
    /// </summary>
    public async Task GetAll_WhenCustomPageSize_ReturnsSmallerPages()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var items = Enumerable.Range(1, 3).Select(i => new ShipmentResponseDTO
        {
            ShipmentId = i,
            CustomerId = i,
            TrackingNumber = $"TRK-{i:D4}",
            Status = ShipmentStatus.Draft
        }).ToList();

        var paginatedResponse = new PaginatedResponse<ShipmentResponseDTO>
        {
            Data = items,
            PageNumber = 1,
            PageSize = 3,
            TotalItems = 12,
            TotalPages = 4,
            HasNextPage = true,
            HasPreviousPage = false
        };

        _shipmentServiceMock.Setup(s => s.GetShipments(1, 3)).ReturnsAsync(paginatedResponse);

        var result = await _controller.GetAll(1, 3);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<ShipmentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.PageSize, Is.EqualTo(3));
        Assert.That(returned.TotalPages, Is.EqualTo(4));
    }

    private static CreateShipmentDTO BuildCreateShipmentDto()
    {
        return new CreateShipmentDTO
        {
            SenderName = "Rajesh Kumar",
            ReceiverName = "Arjun Singh",
            ServiceType = "Express",
            SenderAddress = new Address { Street = "Plot 789, Block C, Sector 45", City = "Delhi", State = "DL", PostalCode = "110001", Country = "IN" },
            ReceiverAddress = new Address { Street = "Flat 302, Building 7, Zone A", City = "Mumbai", State = "MH", PostalCode = "400001", Country = "IN" },
            Packages =
            [
                new PackageDTO
                {
                    Weight = 1.5m,
                    Length = 10,
                    Width = 8,
                    Height = 6,
                    Description = "Computer Electronics"
                }
            ]
        };
    }

    private static void SetUser(ControllerBase controller, int? userId, bool isAdmin)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "UnitTestAuth"))
            }
        };
    }
}


