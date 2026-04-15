using SmartShip.ShipmentService.DTOs;

namespace SmartShip.ShipmentService.Helpers;

/// <summary>
/// Domain model for shipping rate calculator.
/// </summary>
public static class ShippingRateCalculator
{
    /// <summary>
    /// Processes calculate.
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


