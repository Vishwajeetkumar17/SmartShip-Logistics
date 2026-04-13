/// <summary>
/// Provides backend implementation for PickupScheduleDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;
using SmartShip.Shared.Common.Helpers;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents PickupScheduleDTO.
/// </summary>
public class PickupScheduleDTO : IValidatableObject
{
    public DateTime PickupDate { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Executes Validate.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PickupDate <= TimeZoneHelper.GetCurrentUtcTime())
        {
            yield return new ValidationResult("PickupDate must be in the future", new[] { nameof(PickupDate) });
        }
    }
}


