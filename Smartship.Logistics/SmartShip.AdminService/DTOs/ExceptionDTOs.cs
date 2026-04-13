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
    [JsonIgnore]
    public int ShipmentId { get; set; }

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
    public int ExceptionId { get; set; }
    public int ShipmentId { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}


