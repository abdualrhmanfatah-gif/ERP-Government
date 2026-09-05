namespace ERP_Government.Application.Documents.Common;

public interface IFileStorageService
{
    Task<string> SaveStreamAsync(string fileName, Stream content, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string filePath, CancellationToken ct = default);
}
