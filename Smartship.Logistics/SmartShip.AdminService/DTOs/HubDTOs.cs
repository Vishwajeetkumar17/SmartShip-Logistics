using System.ComponentModel.DataAnnotations;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Data transfer model for hub base payloads.
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
/// Data transfer model for create hub payloads.
/// </summary>
public class CreateHubDTO : HubBaseDTO { }
/// <summary>
/// Data transfer model for update hub payloads.
/// </summary>
public class UpdateHubDTO : HubBaseDTO { }

/// <summary>
/// Data transfer model for hub response payloads.
/// </summary>
public class HubResponseDTO : HubBaseDTO
{
    public int HubId { get; set; }
    public DateTime CreatedAt { get; set; }
}


