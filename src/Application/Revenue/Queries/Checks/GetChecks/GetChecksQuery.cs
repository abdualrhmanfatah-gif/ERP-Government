using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.Checks.GetChecks;

[Authorize(Policy = PermissionCodes.ChecksView)]
public class GetChecksQuery : IRequest<Result<List<CheckDto>>>
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public CheckStatus? Status { get; init; }
}

public class GetChecksQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetChecksQuery, Result<List<CheckDto>>>
{
    public async Task<Result<List<CheckDto>>> Handle(
        GetChecksQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ReceiptVouchers
            .Include(v => v.Checks)
            .Where(v => v.PaymentMethod == PaymentMethod.Check
                && v.VoucherDate >= request.From
                && v.VoucherDate <= request.To)
            .SelectMany(v => v.Checks.Select(c => new CheckDto
            {
                Id = c.Id,
                ReceiptVoucherId = c.ReceiptVoucherId,
                BankName = c.BankName,
                CheckNumber = c.CheckNumber,
                CheckDate = c.CheckDate,
                Amount = c.Amount,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                ClearedAt = c.ClearedAt,
                BouncedAt = c.BouncedAt,
                ReplacementVoucherId = c.ReplacementVoucherId,
                Created = c.Created
            }));

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var checks = await query
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);

        return Result<List<CheckDto>>.Success(checks);
    }
}
