using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Revenue.Common.DTOs;

namespace ERP_Government.Application.Revenue.Queries.Checks.GetCheckById;

[Authorize(Policy = PermissionCodes.ChecksView)]
public class GetCheckByIdQuery : IRequest<Result<CheckDetailDto>>
{
    public int Id { get; init; }
}

public class GetCheckByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCheckByIdQuery, Result<CheckDetailDto>>
{
    public async Task<Result<CheckDetailDto>> Handle(
        GetCheckByIdQuery request,
        CancellationToken cancellationToken)
    {
        var check = await context.Checks
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (check is null)
            return Result<CheckDetailDto>.Failure(new[] { "Check not found." });

        var dto = new CheckDetailDto
        {
            CheckId = check.Id,
            BankName = check.BankName,
            CheckNumber = check.CheckNumber,
            CheckDate = check.CheckDate,
            Amount = check.Amount,
            Status = check.Status,
            ClearedAt = check.ClearedAt
        };

        return Result<CheckDetailDto>.Success(dto);
    }
}
