/// <summary>
/// Provides backend implementation for HubDTOs.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents HubBaseDTO.
/// </summary>
public class HubBaseDTO
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    [Required]
    [MaxLength(500)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Address is required")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact number.
    /// </summary>
    [MaxLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the manager name.
    /// </summary>
    [MaxLength(100)]
    public string ManagerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the is active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents CreateHubDTO.
/// </summary>
public class CreateHubDTO : HubBaseDTO { }
/// <summary>
/// Represents UpdateHubDTO.
/// </summary>
public class UpdateHubDTO : HubBaseDTO { }

/// <summary>
/// Represents HubResponseDTO.
/// </summary>
public class HubResponseDTO : HubBaseDTO
{
    /// <summary>
    /// Gets or sets the hub id.
    /// </summary>
    public int HubId { get; set; }
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}


