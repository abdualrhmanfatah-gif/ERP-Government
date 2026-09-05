using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Enums;

namespace ERP_Government.Application.Revenue.Queries.DepositSlips.GetDepositSlips;

[Authorize(Policy = PermissionCodes.DepositSlipsView)]
public class GetDepositSlipsQuery : IRequest<Result<List<DepositSlipDto>>>
{
    public DepositSlipStatus? Status { get; init; }
    public FormType? FormType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetDepositSlipsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDepositSlipsQuery, Result<List<DepositSlipDto>>>
{
    public async Task<Result<List<DepositSlipDto>>> Handle(
        GetDepositSlipsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.DepositSlips
            .Include(s => s.ReceiptVouchers)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.FormType.HasValue)
            query = query.Where(s => s.FormType == request.FormType.Value);

        if (request.FromDate.HasValue)
            query = query.Where(s => s.SlipDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(s => s.SlipDate <= request.ToDate.Value);

        var slips = await query
            .OrderByDescending(s => s.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new DepositSlipDto
            {
                Id = s.Id,
                SlipNumber = s.SlipNumber,
                SlipDate = s.SlipDate,
                FormType = s.FormType,
                Status = s.Status,
                StatusName = s.Status.ToString(),
                TotalAmount = s.TotalAmount,
                ApprovedById = s.ApprovedById,
                ApprovedAt = s.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<DepositSlipDto>>.Success(slips);
    }
}
