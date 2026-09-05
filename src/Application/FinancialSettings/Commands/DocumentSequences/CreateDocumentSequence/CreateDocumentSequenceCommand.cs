using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.DocumentSequences.CreateDocumentSequence;

[Authorize(Policy = PermissionCodes.DocumentSequencesCreate)]
public class CreateDocumentSequenceCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public int? FiscalYearId { get; init; }
    public ResetPolicy ResetPolicy { get; init; } = ResetPolicy.Yearly;
}

public class CreateDocumentSequenceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateDocumentSequenceCommand, Result>
{
    public async Task<Result> Handle(
        CreateDocumentSequenceCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.DocumentSequences
            .AnyAsync(x => x.Name == request.Name, cancellationToken);

        if (exists)
            return Result.Failure(["Sequence name already exists."]);

        var entity = new DocumentSequence
        {
            Name = request.Name,
            DocumentType = request.DocumentType,
            FiscalYearId = request.FiscalYearId,
            CurrentNumber = 1,
            ResetPolicy = request.ResetPolicy,
            IsActive = true
        };

        context.DocumentSequences.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
