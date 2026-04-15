using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SmartShip.Shared.Common.Exceptions;

namespace SmartShip.DocumentService.Storage;

/// <summary>
/// Local web-root file storage for uploaded shipment documents and proof images.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes paths from the host environment web root for uploads.
    /// </summary>
    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Persists the upload under wwwroot/uploads/{subFolder} and returns a web-relative URL path.
    /// </summary>
    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0)
        {
            throw new RequestValidationException("Invalid file.");
        }

        var webRootPath = EnsureWebRootPath();
        var safeFolderName = string.IsNullOrWhiteSpace(subFolder) ? "general" : subFolder.Trim().ToLowerInvariant();
        var uploadsFolder = Path.Combine(webRootPath, "uploads", safeFolderName);
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);

        return $"/uploads/{safeFolderName}/{uniqueFileName}";
    }

    /// <summary>
    /// Deletes a previously stored file when the relative path resolves under web root.
    /// </summary>
    public Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        var physicalPath = ResolvePhysicalPath(filePath);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns whether a file still exists on disk for the given web-relative path.
    /// </summary>
    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        var physicalPath = ResolvePhysicalPath(filePath);
        return File.Exists(physicalPath);
    }

    private string EnsureWebRootPath()
    {
        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(webRootPath);
        return webRootPath;
    }

    private string ResolvePhysicalPath(string filePath)
    {
        var webRootPath = EnsureWebRootPath();
        var relativePath = filePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var combinedPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var rootPath = Path.GetFullPath(webRootPath);

        if (!combinedPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new RequestValidationException("Invalid file path.");
        }

        return combinedPath;
    }
}


