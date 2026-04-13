/// <summary>
/// Provides backend implementation for Package.
/// </summary>

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents Package.
/// </summary>
public class Package
{
    public int PackageId { get; set; }

    public int ShipmentId { get; set; }

    public decimal Weight { get; set; }

    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public string Description { get; set; } = string.Empty;

    public Shipment Shipment { get; set; } = null!;
}


