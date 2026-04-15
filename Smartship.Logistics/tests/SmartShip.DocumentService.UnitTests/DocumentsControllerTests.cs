using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartShip.DocumentService.Controllers;
using SmartShip.DocumentService.DTOs;
using SmartShip.DocumentService.Services;
using SmartShip.Shared.Common.Extensions;
using SmartShip.Shared.DTOs;

namespace SmartShip.DocumentService.UnitTests;

/// <summary>
/// Domain model for documents controller tests.
/// </summary>
    [TestFixture]
/// <summary>
/// Domain model for documents controller tests.
/// </summary>
public class DocumentsControllerTests
{
    private Mock<IDocumentService> _serviceMock = null!;
    private DocumentsController _controller = null!;

    /// <summary>
    /// Sets up.
    /// </summary>
    [SetUp]
    /// <summary>
    /// Sets up.
    /// </summary>
    public void SetUp()
    {
        _serviceMock = new Mock<IDocumentService>(MockBehavior.Strict);
        _controller = new DocumentsController(_serviceMock.Object);
    }

    /// <summary>
    /// Uploads document when no user claim returns unauthorized.
    /// </summary>
    [Test]
    /// <summary>
    /// Uploads document when no user claim returns unauthorized.
    /// </summary>
    public async Task UploadDocument_WhenNoUserClaim_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);

        var result = await _controller.UploadDocument(new UploadDocumentDTO());

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    /// <summary>
    /// Uploads document when user exists calls service and returns ok.
    /// </summary>
    [Test]
    /// <summary>
    /// Uploads document when user exists calls service and returns ok.
    /// </summary>
    public async Task UploadDocument_WhenUserExists_CallsServiceAndReturnsOk()
    {
        SetUser(_controller, userId: 11, isAdmin: false);
        var dto = new UploadDocumentDTO { ShipmentId = 100 };
        var serviceResponse = new DocumentResponseDTO { DocumentId = 1, CustomerId = 11, DocumentType = "General" };

        _serviceMock.Setup(s => s.UploadDocumentAsync(dto, "General", 11)).ReturnsAsync(serviceResponse);

        var result = await _controller.UploadDocument(dto);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(serviceResponse));
    }

    /// <summary>
    /// Returns document when not owner returns forbid.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns document when not owner returns forbid.
    /// </summary>
    public async Task GetDocument_WhenNotOwner_ReturnsForbid()
    {
        SetUser(_controller, userId: 7, isAdmin: false);
        _serviceMock.Setup(s => s.GetDocumentByIdAsync(9)).ReturnsAsync(new DocumentResponseDTO { DocumentId = 9, CustomerId = 8 });

        var result = await _controller.GetDocument(9);

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    /// <summary>
    /// Returns documents by shipment when no user claim returns unauthorized.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns documents by shipment when no user claim returns unauthorized.
    /// </summary>
    public async Task GetDocumentsByShipment_WhenNoUserClaim_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);
        var emptyResponse = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = new List<DocumentResponseDTO>(),
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 0,
            TotalPages = 0
        };
        _serviceMock.Setup(s => s.GetDocumentsByShipmentAsync(20, 1, 5)).ReturnsAsync(emptyResponse);

        var result = await _controller.GetDocumentsByShipment(20);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    /// <summary>
    /// Returns documents by shipment when non admin filters to current user.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns documents by shipment when non admin filters to current user.
    /// </summary>
    public async Task GetDocumentsByShipment_WhenNonAdmin_FiltersToCurrentUser()
    {
        SetUser(_controller, userId: 4, isAdmin: false);
        var docs = new List<DocumentResponseDTO>
        {
            new() { DocumentId = 1, CustomerId = 4, ShipmentId = 20 },
            new() { DocumentId = 2, CustomerId = 9, ShipmentId = 20 }
        };
        var response = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = docs,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = docs.Count,
            TotalPages = 1
        };
        _serviceMock.Setup(s => s.GetDocumentsByShipmentAsync(20, 1, 5)).ReturnsAsync(response);

        var result = await _controller.GetDocumentsByShipment(20);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var paginatedResponse = ok!.Value as PaginatedResponse<DocumentResponseDTO>;
        Assert.That(paginatedResponse, Is.Not.Null);
        Assert.That(paginatedResponse!.Data.Count, Is.EqualTo(1));
        Assert.That(paginatedResponse.Data[0].CustomerId, Is.EqualTo(4));
    }

    /// <summary>
    /// Returns delivery proof when no user claim returns unauthorized.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns delivery proof when no user claim returns unauthorized.
    /// </summary>
    public async Task GetDeliveryProof_WhenNoUserClaim_ReturnsUnauthorized()
    {
        SetUser(_controller, userId: null, isAdmin: false);

        var result = await _controller.GetDeliveryProof(45);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _serviceMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Returns delivery proof when user not owner returns forbid.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns delivery proof when user not owner returns forbid.
    /// </summary>
    public async Task GetDeliveryProof_WhenUserNotOwner_ReturnsForbid()
    {
        SetUser(_controller, userId: 3, isAdmin: false);
        var docs = new List<DocumentResponseDTO> { new() { DocumentId = 1, CustomerId = 99, ShipmentId = 45 } };
        var response = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = docs,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = docs.Count,
            TotalPages = 1
        };
        _serviceMock
            .Setup(s => s.GetDocumentsByShipmentAsync(45, 1, 5))
            .ReturnsAsync(response);

        var result = await _controller.GetDeliveryProof(45);

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    /// <summary>
    /// Returns delivery proof when user owns shipment returns ok.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns delivery proof when user owns shipment returns ok.
    /// </summary>
    public async Task GetDeliveryProof_WhenUserOwnsShipment_ReturnsOk()
    {
        SetUser(_controller, userId: 3, isAdmin: false);
        var proof = new DeliveryProofResponseDTO { ProofId = 1, ShipmentId = 45, SignerName = "Recipient" };
        var docs = new List<DocumentResponseDTO> { new() { DocumentId = 1, CustomerId = 3, ShipmentId = 45 } };
        var response = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = docs,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = docs.Count,
            TotalPages = 1
        };
        _serviceMock
            .Setup(s => s.GetDocumentsByShipmentAsync(45, 1, 5))
            .ReturnsAsync(response);
        _serviceMock.Setup(s => s.GetDeliveryProofAsync(45)).ReturnsAsync(proof);

        var result = await _controller.GetDeliveryProof(45);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(proof));
    }

    /// <summary>
    /// Returns documents by shipment when called returns paginated documents.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns documents by shipment when called returns paginated documents.
    /// </summary>
    public async Task GetDocumentsByShipment_WhenCalled_ReturnsPaginatedDocuments()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var docs = Enumerable.Range(1, 12).Select(i => new DocumentResponseDTO
        {
            DocumentId = i,
            CustomerId = 1,
            ShipmentId = 1,
            DocumentType = "Invoice"
        }).ToList();

        var response = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = docs.Take(5).ToList(),
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = false
        };

        _serviceMock.Setup(s => s.GetDocumentsByShipmentAsync(1, 1, 5)).ReturnsAsync(response);

        var result = await _controller.GetDocumentsByShipment(1, 1, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<DocumentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.Data.Count, Is.EqualTo(5));
        Assert.That(returned.TotalPages, Is.EqualTo(3));
        Assert.That(returned.HasNextPage, Is.True);
    }

    /// <summary>
    /// Returns documents by shipment when page number2 returns second page.
    /// </summary>
    [Test]
    /// <summary>
    /// Returns documents by shipment when page number2 returns second page.
    /// </summary>
    public async Task GetDocumentsByShipment_WhenPageNumber2_ReturnsSecondPage()
    {
        SetUser(_controller, userId: 1, isAdmin: true);
        var docs = Enumerable.Range(6, 5).Select(i => new DocumentResponseDTO
        {
            DocumentId = i,
            CustomerId = 1,
            ShipmentId = 1,
            DocumentType = "POD"
        }).ToList();

        var response = new PaginatedResponse<DocumentResponseDTO>
        {
            Data = docs,
            PageNumber = 2,
            PageSize = 5,
            TotalItems = 12,
            TotalPages = 3,
            HasNextPage = true,
            HasPreviousPage = true
        };

        _serviceMock.Setup(s => s.GetDocumentsByShipmentAsync(1, 2, 5)).ReturnsAsync(response);

        var result = await _controller.GetDocumentsByShipment(1, 2, 5);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returned = okResult!.Value as PaginatedResponse<DocumentResponseDTO>;
        Assert.That(returned, Is.Not.Null);
        Assert.That(returned!.PageNumber, Is.EqualTo(2));
        Assert.That(returned.HasNextPage, Is.True);
        Assert.That(returned.HasPreviousPage, Is.True);
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


