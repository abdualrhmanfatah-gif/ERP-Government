using ERP_Government.Application.Documents.Common;

namespace ERP_Government.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveStreamAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy/MM"), $"{Guid.NewGuid():N}_{fileName}");
        var fullPath = Path.Combine(_basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }
}
