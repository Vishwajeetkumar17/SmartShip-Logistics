/// <summary>
/// Provides backend implementation for ShipmentsController.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.Common.Extensions;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Helpers;
using SmartShip.ShipmentService.Services;

namespace SmartShip.ShipmentService.Controllers;

[ApiController]
[Route("api/shipments")]
/// <summary>
/// Represents ShipmentsController.
/// </summary>
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;

    public ShipmentsController(IShipmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    /// <summary>
    /// Executes Create.
    /// </summary>
    public async Task<IActionResult> Create([FromBody] CreateShipmentDTO dto)
    {
        if (!User.IsAdmin())
        {
            if (!User.TryGetCustomerId(out var customerId))
            {
                return Unauthorized();
            }

            dto.CustomerId = customerId;
        }

        var shipment = await _service.CreateShipment(dto);
        return Ok(shipment);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes GetAll.
    /// </summary>
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(await _service.GetShipments(pageNumber, pageSize));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    /// <summary>
    /// Executes Get.
    /// </summary>
    public async Task<IActionResult> Get(int id)
    {
        var shipment = await _service.GetShipment(id);
        if (shipment == null)
        {
            return NotFound();
        }

        if (!User.IsAdmin())
        {
            if (!User.TryGetCustomerId(out var customerId) || shipment.CustomerId != customerId)
            {
                return Forbid();
            }
        }

        return Ok(shipment);
    }

    [HttpGet("tracking/{trackingNumber}")]
    [Authorize]
    /// <summary>
    /// Executes GetByTrackingNumber.
    /// </summary>
    public async Task<IActionResult> GetByTrackingNumber(string trackingNumber)
    {
        var shipment = await _service.GetShipmentByTrackingNumber(trackingNumber);
        if (shipment == null)
        {
            return NotFound();
        }

        if (!User.IsAdmin())
        {
            if (!User.TryGetCustomerId(out var customerId) || shipment.CustomerId != customerId)
            {
                return Forbid();
            }
        }

        return Ok(shipment);
    }

    [HttpGet("my")]
    [Authorize]
    /// <summary>
    /// Executes GetMyShipments.
    /// </summary>
    public async Task<IActionResult> GetMyShipments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        return Ok(await _service.GetCustomerShipments(customerId, pageNumber, pageSize));
    }

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes GetCustomerShipments.
    /// </summary>
    public async Task<IActionResult> GetCustomerShipments(int customerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(await _service.GetCustomerShipments(customerId, pageNumber, pageSize));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes Update.
    /// </summary>
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShipmentDTO dto)
    {
        await _service.UpdateShipment(id, dto);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes Delete.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteShipment(id);
        return Ok();
    }

    [HttpDelete("{id:int}/my")]
    [Authorize]
    /// <summary>
    /// Executes DeleteMyShipment.
    /// </summary>
    public async Task<IActionResult> DeleteMyShipment(int id)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        await _service.DeleteCustomerShipment(id, customerId);
        return Ok();
    }

    [HttpPut("{id:int}/book")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes BookShipment.
    /// </summary>
    public async Task<IActionResult> BookShipment(int id, [FromBody] BookShipmentDTO dto)
    {
        await _service.BookShipment(id, dto);
        return Ok();
    }

    [HttpPut("{id:int}/pickup")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes Pickup.
    /// </summary>
    public async Task<IActionResult> Pickup(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.PickedUp, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/in-transit")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes InTransit.
    /// </summary>
    public async Task<IActionResult> InTransit(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.InTransit, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/out-for-delivery")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes OutForDelivery.
    /// </summary>
    public async Task<IActionResult> OutForDelivery(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.OutForDelivery, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/deliver")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes Deliver.
    /// </summary>
    public async Task<IActionResult> Deliver(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.Delivered, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/schedule-pickup")]
    [Authorize]
    /// <summary>
    /// Executes SchedulePickup.
    /// </summary>
    public async Task<IActionResult> SchedulePickup(int id, [FromBody] PickupScheduleDTO dto)
    {
        if (!User.IsAdmin())
        {
            var shipment = await _service.GetShipment(id);
            if (shipment == null)
            {
                return NotFound();
            }

            if (!User.TryGetCustomerId(out var customerId) || shipment.CustomerId != customerId)
            {
                return Forbid();
            }
        }

        await _service.SchedulePickup(id, dto);
        return Ok();
    }

    [HttpPost("{id:int}/raise-issue")]
    [Authorize]
    /// <summary>
    /// Executes RaiseIssue.
    /// </summary>
    public async Task<IActionResult> RaiseIssue(int id, [FromBody] ShipmentIssueDTO dto)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Unauthorized();
        }

        var shipment = await _service.GetShipment(id);
        if (shipment == null)
        {
            return NotFound();
        }

        if (shipment.CustomerId != customerId)
        {
            return Forbid();
        }

        await _service.RaiseIssueAsync(id, customerId, dto);
        return Ok(new { message = "Issue submitted successfully." });
    }

    [HttpGet("{id:int}/pickup-details")]
    [Authorize]
    /// <summary>
    /// Executes PickupDetails.
    /// </summary>
    public async Task<IActionResult> PickupDetails(int id)
    {
        var shipment = await _service.GetShipment(id);
        if (shipment == null || shipment.PickupSchedule == null)
        {
            return NotFound();
        }

        if (!User.IsAdmin())
        {
            if (!User.TryGetCustomerId(out var customerId) || shipment.CustomerId != customerId)
            {
                return Forbid();
            }
        }

        return Ok(shipment.PickupSchedule);
    }

    [HttpPost("calculate-rate")]
    /// <summary>
    /// Executes CalculateRate.
    /// </summary>
    public IActionResult CalculateRate([FromBody] RateRequestDTO dto)
    {
        var price = ShippingRateCalculator.Calculate(dto);
        return Ok(new { price });
    }

    [HttpGet("services")]
    /// <summary>
    /// Executes GetServices.
    /// </summary>
    public IActionResult GetServices()
    {
        return Ok(new[]
        {
            new { name = "Standard", delivery = "3-5 days" },
            new { name = "Express", delivery = "1-2 days" },
            new { name = "Economy", delivery = "5-7 days" }
        });
    }
}


