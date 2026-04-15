using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.Common.Extensions;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Services;

namespace SmartShip.ShipmentService.Controllers;

[ApiController]
[Route("api/shipments/{shipmentId:int}/packages")]
/// <summary>
/// Exposes packages endpoints for SmartShip logistics workflows.
/// </summary>
public class PackagesController : ControllerBase
{
    private readonly IPackageService _service;
    private readonly IShipmentService _shipmentService;

    public PackagesController(IPackageService service, IShipmentService shipmentService)
    {
        _service = service;
        _shipmentService = shipmentService;
    }

    [HttpPost]
    [Authorize]
    /// <summary>
    /// Adds package.
    /// </summary>
    public async Task<IActionResult> AddPackage(int shipmentId, [FromBody] PackageDTO dto)
    {
        var accessResult = await EnsureShipmentAccess(shipmentId);
        if (accessResult != null)
        {
            return accessResult;
        }

        await _service.AddPackage(shipmentId, dto);
        return Ok();
    }

    [HttpGet]
    [Authorize]
    /// <summary>
    /// Returns packages.
    /// </summary>
    public async Task<IActionResult> GetPackages(int shipmentId)
    {
        var accessResult = await EnsureShipmentAccess(shipmentId);
        if (accessResult != null)
        {
            return accessResult;
        }

        var packages = await _service.GetPackages(shipmentId);
        return Ok(packages);
    }

    [HttpPut("{packageId:int}")]
    [Authorize]
    /// <summary>
    /// Updates package.
    /// </summary>
    public async Task<IActionResult> UpdatePackage(int shipmentId, int packageId, [FromBody] PackageDTO dto)
    {
        var accessResult = await EnsureShipmentAccess(shipmentId);
        if (accessResult != null)
        {
            return accessResult;
        }

        await _service.UpdatePackage(shipmentId, packageId, dto);
        return Ok();
    }

    [HttpDelete("{packageId:int}")]
    [Authorize]
    /// <summary>
    /// Deletes package.
    /// </summary>
    public async Task<IActionResult> DeletePackage(int shipmentId, int packageId)
    {
        var accessResult = await EnsureShipmentAccess(shipmentId);
        if (accessResult != null)
        {
            return accessResult;
        }

        await _service.DeletePackage(shipmentId, packageId);
        return Ok();
    }

    private async Task<IActionResult?> EnsureShipmentAccess(int shipmentId)
    {
        var shipment = await _shipmentService.GetShipment(shipmentId);
        if (shipment == null)
        {
            return NotFound();
        }

        if (User.IsAdmin())
        {
            return null;
        }

        if (!User.TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        return shipment.CustomerId == customerId ? null : Forbid();
    }
}


