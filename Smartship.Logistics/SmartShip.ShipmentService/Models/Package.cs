/// <summary>
/// Provides backend implementation for Package.
/// </summary>

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents Package.
/// </summary>
public class Package
{
    /// <summary>
    /// Gets or sets the package id.
    /// </summary>
    public int PackageId { get; set; }

    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }

    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shipment.
    /// </summary>
    public Shipment Shipment { get; set; } = null!;
}


