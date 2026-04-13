/// <summary>
/// Provides backend implementation for AuthControllerTests.
/// </summary>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using SmartShip.IdentityService.Configurations;
using SmartShip.IdentityService.Controllers;
using SmartShip.IdentityService.DTOs;
using SmartShip.IdentityService.Services;
using SmartShip.Shared.DTOs;

namespace SmartShip.IdentityService.UnitTests;

[TestFixture]
/// <summary>
/// Represents AuthControllerTests.
/// </summary>
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock = null!;

    [SetUp]
    /// <summary>
    /// Executes SetUp.
    /// </summary>
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>(MockBehavior.Strict);
    }

    [Test]
    /// <summary>
    /// Executes Logout_WhenUserIdClaimMissing_ReturnsUnauthorized.
    /// </summary>
    public async Task Logout_WhenUserIdClaimMissing_ReturnsUnauthorized()
    {
        var controller = BuildController(apiKey: "internal-key");
        SetUser(controller, userId: null, isAdmin: false);

        var result = await controller.Logout();

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _authServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    /// <summary>
    /// Executes Logout_WhenUserIdPresent_CallsServiceAndReturnsOk.
    /// </summary>
    public async Task Logout_WhenUserIdPresent_CallsServiceAndReturnsOk()
    {
        var controller = BuildController(apiKey: "internal-key");
        SetUser(controller, userId: 14, isAdmin: false);
        _authServiceMock.Setup(s => s.LogoutAsync(14)).Returns(Task.CompletedTask);

        var result = await controller.Logout();

        Assert.That(result, Is.TypeOf<OkResult>());
        _authServiceMock.Verify(s => s.LogoutAsync(14), Times.Once);
    }

    [Test]
    /// <summary>
    /// Executes Profile_WhenUserIdMissing_ReturnsUnauthorized.
    /// </summary>
    public async Task Profile_WhenUserIdMissing_ReturnsUnauthorized()
    {
        var controller = BuildController(apiKey: "internal-key");
        SetUser(controller, userId: null, isAdmin: false);

        var result = await controller.Profile();

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    /// <summary>
    /// Executes RequestSignupOtp_CallsServiceAndReturnsOk.
    /// </summary>
    public async Task RequestSignupOtp_CallsServiceAndReturnsOk()
    {
        var controller = BuildController(apiKey: "internal-key");
        _authServiceMock.Setup(s => s.RequestSignupOtpAsync(It.IsAny<RegisterDTO>())).Returns(Task.CompletedTask);

        var result = await controller.RequestSignupOtp(new RegisterDTO { Email = "rajesh.kumar@indiamail.com", Name = "Rajesh Kumar", Password = "P@ssw0rd!" });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _authServiceMock.Verify(s => s.RequestSignupOtpAsync(It.IsAny<RegisterDTO>()), Times.Once);
    }

    [Test]
    /// <summary>
    /// Executes GetUserContactInternal_WhenApiKeyConfigMissing_Returns500.
    /// </summary>
    public async Task GetUserContactInternal_WhenApiKeyConfigMissing_Returns500()
    {
        var controller = BuildController(apiKey: string.Empty);

        var result = await controller.GetUserContactInternal(5, "any");

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    /// <summary>
    /// Executes GetUserContactInternal_WhenApiKeyInvalid_ReturnsUnauthorized.
    /// </summary>
    public async Task GetUserContactInternal_WhenApiKeyInvalid_ReturnsUnauthorized()
    {
        var controller = BuildController(apiKey: "expected");

        var result = await controller.GetUserContactInternal(5, "wrong");

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    /// <summary>
    /// Executes GetUserContactInternal_WhenApiKeyValid_ReturnsMappedContact.
    /// </summary>
    public async Task GetUserContactInternal_WhenApiKeyValid_ReturnsMappedContact()
    {
        var controller = BuildController(apiKey: "expected");
        _authServiceMock.Setup(s => s.GetUserByIdAsync(5)).ReturnsAsync(new AuthDTO
        {
            UserId = 5,
            Name = "Priya Sharma",
            Email = "priya.sharma@indiamail.com",
            Role = "CUSTOMER"
        });

        var result = await controller.GetUserContactInternal(5, "expected");

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var contact = ok!.Value as UserContactDTO;
        Assert.That(contact, Is.Not.Null);
        Assert.That(contact!.UserId, Is.EqualTo(5));
        Assert.That(contact.Name, Is.EqualTo("Priya Sharma"));
    }

    [Test]
    /// <summary>
    /// Executes AssignRole_CallsServiceAndReturnsOk.
    /// </summary>
    public async Task AssignRole_CallsServiceAndReturnsOk()
    {
        var controller = BuildController(apiKey: "internal-key");
        _authServiceMock.Setup(s => s.AssignRoleAsync(8, 3)).Returns(Task.CompletedTask);

        var result = await controller.AssignRole(8, new AssignRoleDTO { RoleId = 3 });

        Assert.That(result, Is.TypeOf<OkResult>());
        _authServiceMock.Verify(s => s.AssignRoleAsync(8, 3), Times.Once);
    }

    [Test]
    /// <summary>
    /// Executes GetUsers_WhenCalled_ReturnsPaginatedUsers.
    /// </summary>
    public async Task GetUsers_WhenCalled_ReturnsPaginatedUsers()
    {
        var controller = BuildController(apiKey: "internal-key");
        SetUser(controller, userId: 1, isAdmin: true);
        var users = Enumerable.Range(1, 12).Select(i => new AuthDTO
        {
            UserId = i,
            Email = $"user{i}@example.com",
            Name = $"User {i}",
            Role = "CUSTOMER"
        }).ToList();

        var paginatedResponse = new PaginatedResponse<AuthDTO>
        {
            Data = users.Take(5).ToList(),
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = false
        };

        _authServiceMock.Setup(s => s.GetUsersAsync(1, 5)).ReturnsAsync(paginatedResponse);

        var result = await controller.GetUsers(1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<AuthDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.Data.Count, Is.EqualTo(5));
        Assert.That(returned.TotalPages, Is.EqualTo(3));
        Assert.That(returned.HasNextPage, Is.True);
    }

    [Test]
    /// <summary>
    /// Executes GetRoles_WhenCalled_ReturnsPaginatedRoles.
    /// </summary>
    public async Task GetRoles_WhenCalled_ReturnsPaginatedRoles()
    {
        var controller = BuildController(apiKey: "internal-key");
        SetUser(controller, userId: 1, isAdmin: true);
        var roles = new List<RoleDTO>
        {
            new() { RoleId = 1, RoleName = "ADMIN" },
            new() { RoleId = 2, RoleName = "CUSTOMER" },
            new() { RoleId = 3, RoleName = "STAFF" }
        };

        var paginatedResponse = new PaginatedResponse<RoleDTO>
        {
            Data = roles,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 3,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        _authServiceMock.Setup(s => s.GetRolesAsync(1, 5)).ReturnsAsync(paginatedResponse);

        var result = await controller.GetRoles(1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<RoleDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.TotalPages, Is.EqualTo(1));
        Assert.That(returned.HasNextPage, Is.False);
        Assert.That(returned.Data.Count, Is.EqualTo(3));
    }

    private AuthController BuildController(string apiKey)
    {
        var options = Options.Create(new InternalServiceAuthSettings { ApiKey = apiKey });
        return new AuthController(_authServiceMock.Object, options);
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


