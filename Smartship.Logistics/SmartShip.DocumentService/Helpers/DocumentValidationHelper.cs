/// <summary>
/// Provides backend implementation for DocumentValidationHelper.
/// </summary>

using SmartShip.DocumentService.DTOs;
using SmartShip.Shared.Common.Exceptions;

namespace SmartShip.DocumentService.Helpers;

/// <summary>
/// Represents DocumentValidationHelper.
/// </summary>
public static class DocumentValidationHelper
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedDocumentTypes =
    [
        "General",
        "Invoice",
        "Label",
        "Customs"
    ];

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png"
    ];

    private static readonly HashSet<string> AllowedFileExtensions =
    [
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    ];

    /// <summary>
    /// Executes the ValidateUpload operation.
    /// </summary>
    public static void ValidateUpload(UploadDocumentDTO dto, string documentType)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.ShipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        ValidateDocumentType(documentType);
        ValidateFile(dto.File);
    }

    /// <summary>
    /// Executes the ValidateDeliveryProof operation.
    /// </summary>
    public static void ValidateDeliveryProof(int shipmentId, DeliveryProofDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(dto.SignerName))
        {
            throw new RequestValidationException("SignerName is required.");
        }

        ValidateFile(dto.File);
    }

    /// <summary>
    /// Executes the NormalizeDocumentType operation.
    /// </summary>
    public static string NormalizeDocumentType(string documentType)
    {
        ValidateDocumentType(documentType);
        return AllowedDocumentTypes.Single(type => type.Equals(documentType.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Executes the NormalizeText operation.
    /// </summary>
    public static string NormalizeText(string value) => value?.Trim() ?? string.Empty;

    private static void ValidateDocumentType(string documentType)
    {
        if (string.IsNullOrWhiteSpace(documentType) || !AllowedDocumentTypes.Contains(documentType.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            throw new RequestValidationException("Invalid document type.");
        }
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            throw new RequestValidationException("File data is required.");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new RequestValidationException("File size must not exceed 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new RequestValidationException("Unsupported file type.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new RequestValidationException("Unsupported content type.");
        }
    }
}


