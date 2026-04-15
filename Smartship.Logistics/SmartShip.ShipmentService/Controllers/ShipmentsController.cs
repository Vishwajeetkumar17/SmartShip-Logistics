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
/// Shipment lifecycle API: create and query shipments, admin status transitions, customer pickup and issues.
/// </summary>
public class ShipmentsController : ControllerBase
{
    #region Fields
    private readonly IShipmentService _service;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes the shipments controller dependencies.
    /// </summary>
    public ShipmentsController(IShipmentService service)
    {
        _service = service;
    }
    #endregion

    #region Create and Query Endpoints
    [HttpPost]
    [Authorize]
    /// <summary>
    /// Creates a new shipment and scopes customer ownership for non-admin callers.
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
    /// Returns a paginated list of all shipments (admin only).
    /// </summary>
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(await _service.GetShipments(pageNumber, pageSize));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    /// <summary>
    /// Returns shipment details by identifier with role-based ownership checks.
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
    /// Returns a shipment by tracking number.
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
    /// Returns shipments for the authenticated customer.
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
    /// Returns customer shipments.
    /// </summary>
    public async Task<IActionResult> GetCustomerShipments(int customerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(await _service.GetCustomerShipments(customerId, pageNumber, pageSize));
    }
    #endregion

    #region Management Endpoints
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Updates shipment details for administrator-driven corrections.
    /// </summary>
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShipmentDTO dto)
    {
        await _service.UpdateShipment(id, dto);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Deletes a shipment record from the administrative management flow.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteShipment(id);
        return Ok();
    }

    [HttpDelete("{id:int}/my")]
    [Authorize]
    /// <summary>
    /// Deletes my shipment.
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
    #endregion

    #region Status Transition Endpoints
    [HttpPut("{id:int}/book")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Confirms booking details for a shipment after creation (admin workflow).
    /// </summary>
    public async Task<IActionResult> BookShipment(int id, [FromBody] BookShipmentDTO dto)
    {
        await _service.BookShipment(id, dto);
        return Ok();
    }

    [HttpPut("{id:int}/pickup")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Sets shipment status to picked up; optional hub location is recorded on the timeline.
    /// </summary>
    public async Task<IActionResult> Pickup(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.PickedUp, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/in-transit")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Sets shipment status to in transit; optional hub location is recorded on the timeline.
    /// </summary>
    public async Task<IActionResult> InTransit(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.InTransit, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/out-for-delivery")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Sets shipment status to out for delivery; optional hub location is recorded on the timeline.
    /// </summary>
    public async Task<IActionResult> OutForDelivery(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.OutForDelivery, dto?.HubLocation);
        return Ok();
    }

    [HttpPut("{id:int}/deliver")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Marks the shipment as delivered; optional hub location is recorded on the timeline.
    /// </summary>
    public async Task<IActionResult> Deliver(int id, [FromBody] ShipmentStatusUpdateDTO? dto)
    {
        await _service.UpdateStatus(id, ShipmentStatus.Delivered, dto?.HubLocation);
        return Ok();
    }
    #endregion

    #region Customer Action Endpoints
    [HttpPut("{id:int}/schedule-pickup")]
    [Authorize]
    /// <summary>
    /// Sets or updates the customer-requested pickup window for the shipment.
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
    /// Records a customer-reported issue (delay, damage, etc.) for support follow-up.
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
    /// Returns the scheduled pickup details for a shipment when a pickup has been arranged.
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
    #endregion

    #region Utility Endpoints
    [HttpPost("calculate-rate")]
    /// <summary>
    /// Returns an estimated shipping rate from weight, dimensions, and service selection (unauthenticated).
    /// </summary>
    public IActionResult CalculateRate([FromBody] RateRequestDTO dto)
    {
        var price = ShippingRateCalculator.Calculate(dto);
        return Ok(new { price });
    }

    [HttpGet("services")]
    /// <summary>
    /// Returns available shipping service options.
    /// </summary>
    public IActionResult GetServices()
    {
        // Keep this endpoint lightweight so UI can render static service choices quickly.
        return Ok(new[]
        {
            new { name = "Standard", delivery = "3-5 days" },
            new { name = "Express", delivery = "1-2 days" },
            new { name = "Economy", delivery = "5-7 days" }
        });
    }
    #endregion
}


