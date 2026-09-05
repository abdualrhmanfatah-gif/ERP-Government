using ERP_Government.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERP_Government.Application.FinancialSettings.Common.Services;

public class DatabaseTransactionFactory : IDatabaseTransactionFactory
{
    private readonly IApplicationDbContext _context;

    public DatabaseTransactionFactory(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _context.Database.BeginTransactionAsync(cancellationToken);
}
