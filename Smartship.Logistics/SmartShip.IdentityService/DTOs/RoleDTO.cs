namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for role payloads.
    /// </summary>
    public class RoleDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}


