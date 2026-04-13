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
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Address is required")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ManagerName { get; set; } = string.Empty;

    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

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
    public int HubId { get; set; }
    public DateTime CreatedAt { get; set; }
}


