/// <summary>
/// Provides backend implementation for PackagesController.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.Common.Extensions;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Services;

namespace SmartShip.ShipmentService.Controllers;

[ApiController]
[Route("api/shipments/{shipmentId:int}/packages")]
/// <summary>
/// Represents PackagesController.
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
    /// Executes AddPackage.
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
    /// Executes GetPackages.
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
    /// Executes UpdatePackage.
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
    /// Executes DeletePackage.
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


