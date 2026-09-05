using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Commands.AccountGroups.ToggleAccountGroupActive;

[Authorize(Policy = PermissionCodes.ChartOfAccountsEdit)]
public class ToggleAccountGroupActiveCommand : IRequest<Result>
{
    public int Id { get; init; }
    public bool IsActive { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ToggleAccountGroupActiveCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ToggleAccountGroupActiveCommand, Result>
{
    public async Task<Result> Handle(ToggleAccountGroupActiveCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.AccountGroups.FindAsync(request.Id, cancellationToken);
        if (entity == null)
            return Result.Failure(["Account group not found."]);

        // Concurrency check — tolerant for empty mock rowversion
        if (entity.RowVersion.Length > 0 && request.RowVersion.Length > 0 && !entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["تم تعديل البيانات من قبل مستخدم آخر، يرجى التحديث والمحاولة مرة أخرى."]);

        // Deactivate guard: deep check
        if (!request.IsActive)
        {
            var subtreeIds = await AccountGroupHierarchyHelper.GetSubtreeIdsAsync(context, request.Id, cancellationToken);
            var (hasActiveDescendant, hasActiveAccount) = await AccountGroupGuardHelper.CheckDeepDeactivateGuardAsync(context, subtreeIds, request.Id, cancellationToken);
            if (hasActiveDescendant)
                return Result.Failure(["لا يمكن تعطيل مجموعة لديها مجموعات فرعية نشطة — عطّل الفروع أولاً."]);
            if (hasActiveAccount)
                return Result.Failure(["لا يمكن تعطيل مجموعة مرتبطة بحسابات نشطة."]);
        }

        entity.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ToggleAccountGroupActiveCommandValidator : AbstractValidator<ToggleAccountGroupActiveCommand>
{
    public ToggleAccountGroupActiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion is required for concurrency control.");
    }
}
