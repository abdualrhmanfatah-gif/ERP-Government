using Microsoft.EntityFrameworkCore.Storage;

namespace ERP_Government.Application.FinancialSettings.Common.Services;

public interface IDatabaseTransactionFactory
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
