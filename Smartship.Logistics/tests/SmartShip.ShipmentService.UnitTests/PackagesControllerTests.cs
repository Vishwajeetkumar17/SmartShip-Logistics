/// <summary>
/// Provides backend implementation for PackagesControllerTests.
/// </summary>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartShip.ShipmentService.Controllers;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Services;

namespace SmartShip.ShipmentService.UnitTests;

/// <summary>
    /// Represents the packages controller tests entity or configuration model.
    /// </summary>
    [TestFixture]
/// <summary>
/// Represents PackagesControllerTests.
/// </summary>
public class PackagesControllerTests
{
    private Mock<IPackageService> _packageServiceMock = null!;
    private Mock<IShipmentService> _shipmentServiceMock = null!;
    private PackagesController _controller = null!;

    /// <summary>
    /// Asynchronously handles the set up process.
    /// </summary>
    [SetUp]
    /// <summary>
    /// Executes the SetUp operation.
    /// </summary>
    public void SetUp()
    {
        _packageServiceMock = new Mock<IPackageService>(MockBehavior.Strict);
        _shipmentServiceMock = new Mock<IShipmentService>(MockBehavior.Strict);
        _controller = new PackagesController(_packageServiceMock.Object, _shipmentServiceMock.Object);
    }

    /// <summary>
    /// Asynchronously handles the add package_when shipment missing_returns not found process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddPackage_WhenShipmentMissing_ReturnsNotFound operation.
    /// </summary>
    public async Task AddPackage_WhenShipmentMissing_ReturnsNotFound()
    {
        SetUser(_controller, userId: 3, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(44)).ReturnsAsync((ShipmentResponseDTO?)null);

        var result = await _controller.AddPackage(44, BuildPackage());

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    /// <summary>
    /// Asynchronously handles the add package_when customer claim missing_returns unauthorized process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddPackage_WhenCustomerClaimMissing_ReturnsUnauthorized operation.
    /// </summary>
    public async Task AddPackage_WhenCustomerClaimMissing_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(44)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 44, CustomerId = 3 });

        var result = await _controller.AddPackage(44, BuildPackage());

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    /// <summary>
    /// Asynchronously handles the add package_when non owner_returns forbid process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddPackage_WhenNonOwner_ReturnsForbid operation.
    /// </summary>
    public async Task AddPackage_WhenNonOwner_ReturnsForbid()
    {
        SetUser(_controller, userId: 5, isAdmin: false);
        _shipmentServiceMock.Setup(s => s.GetShipment(44)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 44, CustomerId = 3 });

        var result = await _controller.AddPackage(44, BuildPackage());

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    /// <summary>
    /// Asynchronously handles the add package_when owner_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddPackage_WhenOwner_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task AddPackage_WhenOwner_CallsServiceAndReturnsOk()
    {
        SetUser(_controller, userId: 3, isAdmin: false);
        var packageDto = BuildPackage();
        _shipmentServiceMock.Setup(s => s.GetShipment(44)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 44, CustomerId = 3 });
        _packageServiceMock.Setup(s => s.AddPackage(44, packageDto)).Returns(Task.CompletedTask);

        var result = await _controller.AddPackage(44, packageDto);

        Assert.That(result, Is.TypeOf<OkResult>());
        _packageServiceMock.Verify(s => s.AddPackage(44, packageDto), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the get packages_when admin_returns packages process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetPackages_WhenAdmin_ReturnsPackages operation.
    /// </summary>
    public async Task GetPackages_WhenAdmin_ReturnsPackages()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var packages = new List<PackageDTO> { BuildPackage() };

        _shipmentServiceMock.Setup(s => s.GetShipment(44)).ReturnsAsync(new ShipmentResponseDTO { ShipmentId = 44, CustomerId = 999 });
        _packageServiceMock.Setup(s => s.GetPackages(44)).ReturnsAsync(packages);

        var result = await _controller.GetPackages(44);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(packages));
    }

    private static PackageDTO BuildPackage()
    {
        return new PackageDTO
        {
            Weight = 2.2m,
            Length = 12,
            Width = 10,
            Height = 8,
            Description = "Footwear and Apparel"
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


