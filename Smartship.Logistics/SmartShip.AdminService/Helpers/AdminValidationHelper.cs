/// <summary>
/// Provides backend implementation for AdminValidationHelper.
/// </summary>

using SmartShip.AdminService.DTOs;
using SmartShip.Shared.Common.Exceptions;
using System.Text.RegularExpressions;

namespace SmartShip.AdminService.Helpers;

/// <summary>
/// Represents AdminValidationHelper.
/// </summary>
public static class AdminValidationHelper
{
    /// <summary>
    /// Executes ValidateHub.
    /// </summary>
    public static void ValidateHub(CreateHubDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        EnsureRequired(dto.Name, "Name");
        EnsureRequired(dto.Address, "Address");
        ValidateHubName(dto.Name);
        ValidateManagerName(dto.ManagerName);
        ValidatePhoneNumber(dto.ContactNumber);
        ValidateEmail(dto.Email);
    }

    /// <summary>
    /// Executes ValidateHub.
    /// </summary>
    public static void ValidateHub(UpdateHubDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        EnsureRequired(dto.Name, "Name");
        EnsureRequired(dto.Address, "Address");
        ValidateHubName(dto.Name);
        ValidateManagerName(dto.ManagerName);
        ValidatePhoneNumber(dto.ContactNumber);
        ValidateEmail(dto.Email);
    }

    /// <summary>
    /// Executes ValidateLocation.
    /// </summary>
    public static void ValidateLocation(CreateLocationDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.HubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        EnsureRequired(dto.Name, "Name");
        EnsureRequired(dto.ZipCode, "ZipCode");
    }

    /// <summary>
    /// Executes ValidateLocation.
    /// </summary>
    public static void ValidateLocation(UpdateLocationDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.HubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        EnsureRequired(dto.Name, "Name");
        EnsureRequired(dto.ZipCode, "ZipCode");
    }

    /// <summary>
    /// Executes NormalizeReason.
    /// </summary>
    public static string NormalizeReason(string reason, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new RequestValidationException($"{fieldName} is required.");
        }

        return reason.Trim();
    }

    private static void EnsureRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequestValidationException($"{fieldName} is required.");
        }
    }

    private static void ValidateHubName(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (!Regex.IsMatch(trimmed, @"^[A-Za-z0-9][A-Za-z0-9\s.'-]{1,99}$"))
        {
            throw new RequestValidationException("Hub name must be 2-100 characters and can contain letters, numbers, spaces, dot, apostrophe, and hyphen.");
        }
    }

    private static void ValidateManagerName(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new RequestValidationException("ManagerName is required.");
        }

        if (!Regex.IsMatch(trimmed, @"^[A-Za-z][A-Za-z\s.'-]{1,99}$"))
        {
            throw new RequestValidationException("Manager name must be 2-100 characters and contain only letters/spaces.");
        }
    }

    private static void ValidatePhoneNumber(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (!Regex.IsMatch(trimmed, @"^\d{10}$"))
        {
            throw new RequestValidationException("Contact number must be exactly 10 digits.");
        }
    }

    private static void ValidateEmail(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new RequestValidationException("Email is required.");
        }

        if (!Regex.IsMatch(trimmed, @"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$"))
        {
            throw new RequestValidationException("Please enter a valid email address.");
        }
    }
}


