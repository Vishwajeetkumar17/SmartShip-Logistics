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
    /// <summary>
    /// Gets or sets the pickup date.
    /// </summary>
    public DateTime PickupDate { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Executes the Validate operation.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PickupDate <= TimeZoneHelper.GetCurrentUtcTime())
        {
            yield return new ValidationResult("PickupDate must be in the future", new[] { nameof(PickupDate) });
        }
    }
}


