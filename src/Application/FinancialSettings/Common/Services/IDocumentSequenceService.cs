namespace ERP_Government.Application.FinancialSettings.Common.Services;

public interface IDocumentSequenceService
{
    Task<string> GenerateNextNumberAsync(string documentType, CancellationToken cancellationToken = default);
}
