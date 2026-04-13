/// <summary>
/// Provides backend implementation for IFileStorageService.
/// </summary>

using Microsoft.AspNetCore.Http;

namespace SmartShip.DocumentService.Storage;

/// <summary>
/// Represents IFileStorageService.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    Task DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
}


