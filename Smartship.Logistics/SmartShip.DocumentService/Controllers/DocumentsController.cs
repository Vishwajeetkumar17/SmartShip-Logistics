/// <summary>
/// Provides backend implementation for DocumentsController.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.DocumentService.DTOs;
using SmartShip.DocumentService.Services;
using SmartShip.Shared.Common.Extensions;

namespace SmartShip.DocumentService.Controllers;

[ApiController]
[Route("api/documents")]
/// <summary>
/// Represents DocumentsController.
/// </summary>
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    [Authorize]
    /// <summary>
    /// Executes UploadDocument.
    /// </summary>
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDTO dto)
    {
        if (!User.TryGetUserId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _service.UploadDocumentAsync(dto, "General", customerId);
        return Ok(result);
    }

    [HttpPost("upload-invoice")]
    [Authorize]
    /// <summary>
    /// Executes UploadInvoice.
    /// </summary>
    public async Task<IActionResult> UploadInvoice([FromForm] UploadDocumentDTO dto)
    {
        if (!User.TryGetUserId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _service.UploadDocumentAsync(dto, "Invoice", customerId);
        return Ok(result);
    }

    [HttpPost("upload-label")]
    [Authorize]
    /// <summary>
    /// Executes UploadLabel.
    /// </summary>
    public async Task<IActionResult> UploadLabel([FromForm] UploadDocumentDTO dto)
    {
        if (!User.TryGetUserId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _service.UploadDocumentAsync(dto, "Label", customerId);
        return Ok(result);
    }

    [HttpPost("upload-customs")]
    [Authorize]
    /// <summary>
    /// Executes UploadCustoms.
    /// </summary>
    public async Task<IActionResult> UploadCustoms([FromForm] UploadDocumentDTO dto)
    {
        if (!User.TryGetUserId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _service.UploadDocumentAsync(dto, "Customs", customerId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    /// <summary>
    /// Executes GetDocument.
    /// </summary>
    public async Task<IActionResult> GetDocument(int id)
    {
        var result = await _service.GetDocumentByIdAsync(id);

        if (!User.IsAdmin())
        {
            if (!User.TryGetUserId(out var currentUserId) || result.CustomerId != currentUserId)
            {
                return Forbid();
            }
        }

        return Ok(result);
    }

    [HttpGet("shipment/{shipmentId:int}")]
    [Authorize]
    /// <summary>
    /// Executes GetDocumentsByShipment.
    /// </summary>
    public async Task<IActionResult> GetDocumentsByShipment(int shipmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetDocumentsByShipmentAsync(shipmentId, pageNumber, pageSize);

        if (!User.IsAdmin())
        {
            if (!User.TryGetUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var filtered = result.Data.Where(d => d.CustomerId == currentUserId).ToList();
            var filteredResponse = filtered.ToPaginatedResponse(pageNumber, pageSize, filtered.Count);
            return Ok(filteredResponse);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId:int}")]
    [Authorize]
    /// <summary>
    /// Executes GetDocumentsByCustomer.
    /// </summary>
    public async Task<IActionResult> GetDocumentsByCustomer(int customerId)
    {
        if (!User.IsAdmin())
        {
            if (!User.TryGetUserId(out var currentUserId) || currentUserId != customerId)
            {
                return Forbid();
            }
        }

        var result = await _service.GetDocumentsByCustomerAsync(customerId);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    /// <summary>
    /// Executes UpdateDocument.
    /// </summary>
    public async Task<IActionResult> UpdateDocument(int id, [FromForm] UploadDocumentDTO dto)
    {
        if (!User.TryGetUserId(out var customerId))
        {
            return Unauthorized();
        }

        var result = await _service.UpdateDocumentAsync(id, dto, customerId, User.IsAdmin());
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes DeleteDocument.
    /// </summary>
    public async Task<IActionResult> DeleteDocument(int id)
    {
        await _service.DeleteDocumentAsync(id);
        return Ok();
    }

    [HttpPost("delivery-proof/{shipmentId:int}")]
    [Authorize(Roles = "DRIVER,ADMIN")]
    /// <summary>
    /// Executes UploadDeliveryProof.
    /// </summary>
    public async Task<IActionResult> UploadDeliveryProof(int shipmentId, [FromForm] DeliveryProofDTO dto)
    {
        var result = await _service.UploadDeliveryProofAsync(shipmentId, dto);
        return Ok(result);
    }

    [HttpGet("delivery-proof/{shipmentId:int}")]
    [Authorize]
    /// <summary>
    /// Executes GetDeliveryProof.
    /// </summary>
    public async Task<IActionResult> GetDeliveryProof(int shipmentId)
    {
        if (!User.IsAdmin())
        {
            if (!User.TryGetUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var shipmentDocumentsResponse = await _service.GetDocumentsByShipmentAsync(shipmentId);
            if (!shipmentDocumentsResponse.Data.Any(d => d.CustomerId == currentUserId))
            {
                return Forbid();
            }
        }

        var result = await _service.GetDeliveryProofAsync(shipmentId);
        return Ok(result);
    }
}


