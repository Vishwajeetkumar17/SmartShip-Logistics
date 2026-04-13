/// <summary>
/// Provides backend implementation for ExceptionDTOs.
/// </summary>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents ResolveExceptionDTO.
/// </summary>
public class ResolveExceptionDTO
{
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    [JsonIgnore]
    public int ShipmentId { get; set; }

    /// <summary>
    /// Gets or sets the resolution notes.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "ResolutionNotes is required")]
    public string ResolutionNotes { get; set; } = string.Empty;
}

/// <summary>
/// Represents ShipmentActionReasonDTO.
/// </summary>
public class ShipmentActionReasonDTO
{
    /// <summary>
    /// Gets or sets the reason.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Represents ExceptionRecordResponseDTO.
/// </summary>
public class ExceptionRecordResponseDTO
{
    /// <summary>
    /// Gets or sets the exception id.
    /// </summary>
    public int ExceptionId { get; set; }
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the exception type.
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the resolved at.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}


