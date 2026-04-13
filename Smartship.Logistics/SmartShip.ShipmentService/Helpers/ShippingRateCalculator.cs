/// <summary>
/// Provides backend implementation for ShippingRateCalculator.
/// </summary>

using SmartShip.ShipmentService.DTOs;

namespace SmartShip.ShipmentService.Helpers;

/// <summary>
/// Represents ShippingRateCalculator.
/// </summary>
public static class ShippingRateCalculator
{
    /// <summary>
    /// Executes the Calculate operation.
    /// </summary>
    public static decimal Calculate(RateRequestDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        const decimal baseRate = 50m;
        var weightCharge = dto.Weight * 10m;

        var serviceMultiplier = dto.ServiceType.Trim() switch
        {
            "Express" => 2m,
            "Standard" => 1m,
            "Economy" => 0.8m,
            _ => throw new ArgumentOutOfRangeException(nameof(dto.ServiceType), "Unsupported service type")
        };

        return decimal.Round((baseRate + weightCharge) * serviceMultiplier, 2, MidpointRounding.AwayFromZero);
    }
}


