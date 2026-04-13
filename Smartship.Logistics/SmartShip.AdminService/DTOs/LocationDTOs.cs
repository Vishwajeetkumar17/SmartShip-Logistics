/// <summary>
/// Provides backend implementation for LocationDTOs.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents LocationBaseDTO.
/// </summary>
public class LocationBaseDTO
{
    /// <summary>
    /// Gets or sets the hub id.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "HubId must be greater than 0")]
    public int HubId { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zip code.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "ZipCode is required")]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the is active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents CreateLocationDTO.
/// </summary>
public class CreateLocationDTO : LocationBaseDTO { }
/// <summary>
/// Represents UpdateLocationDTO.
/// </summary>
public class UpdateLocationDTO : LocationBaseDTO { }

/// <summary>
/// Represents LocationResponseDTO.
/// </summary>
public class LocationResponseDTO : LocationBaseDTO
{
    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public int LocationId { get; set; }
}


