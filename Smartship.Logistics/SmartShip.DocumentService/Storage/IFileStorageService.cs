using Microsoft.AspNetCore.Http;

namespace SmartShip.DocumentService.Storage;

/// <summary>
/// Defines file storage business operations used by the service layer.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    Task DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
}


