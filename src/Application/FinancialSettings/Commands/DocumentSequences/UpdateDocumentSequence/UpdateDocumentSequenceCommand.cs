using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.UpdateDocumentSequence;

[Authorize(Policy = PermissionCodes.DocumentSequencesCreate)]
public class UpdateDocumentSequenceCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public int? FiscalYearId { get; init; }
    public ResetPolicy? ResetPolicy { get; init; }
    public bool? IsActive { get; init; }
}

public class UpdateDocumentSequenceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateDocumentSequenceCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDocumentSequenceCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DocumentSequences
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Document sequence not found."]);

        if (request.Name is not null)
            entity.Name = request.Name;

        if (request.FiscalYearId.HasValue)
            entity.FiscalYearId = request.FiscalYearId;

        if (request.ResetPolicy.HasValue)
            entity.ResetPolicy = request.ResetPolicy.Value;

        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
