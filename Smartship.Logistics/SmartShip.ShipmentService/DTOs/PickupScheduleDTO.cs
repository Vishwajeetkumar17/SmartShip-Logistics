using System.ComponentModel.DataAnnotations;
using SmartShip.Shared.Common.Helpers;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for pickup schedule payloads.
/// </summary>
public class PickupScheduleDTO : IValidatableObject
{
    public DateTime PickupDate { get; set; }
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Validates request data against business rules.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PickupDate <= TimeZoneHelper.GetCurrentUtcTime())
        {
            yield return new ValidationResult("PickupDate must be in the future", new[] { nameof(PickupDate) });
        }
    }
}


