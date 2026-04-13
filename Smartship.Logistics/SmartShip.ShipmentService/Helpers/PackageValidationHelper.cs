/// <summary>
/// Provides backend implementation for PackageValidationHelper.
/// </summary>

using SmartShip.Shared.Common.Exceptions;
using SmartShip.ShipmentService.DTOs;

namespace SmartShip.ShipmentService.Helpers;

/// <summary>
/// Represents PackageValidationHelper.
/// </summary>
public static class PackageValidationHelper
{
    /// <summary>
    /// Executes the ValidatePackage operation.
    /// </summary>
    public static void ValidatePackage(PackageDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Weight <= 0 || dto.Length <= 0 || dto.Width <= 0 || dto.Height <= 0)
        {
            throw new RequestValidationException("Package dimensions and weight must be greater than 0.");
        }
    }
}


