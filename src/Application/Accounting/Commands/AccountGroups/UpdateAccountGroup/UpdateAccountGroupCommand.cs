using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.AccountGroups.UpdateAccountGroup;

[Authorize(Policy = PermissionCodes.ChartOfAccountsEdit)]
public class UpdateAccountGroupCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AccountGroupType Type { get; init; }
    public NormalBalanceType NormalBalance { get; init; }
    public string? Description { get; init; }
    public int? ParentId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateAccountGroupCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAccountGroupCommand, Result>
{
    public async Task<Result> Handle(
        UpdateAccountGroupCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.AccountGroups
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Account group not found."]);

        // Concurrency check (optimistic) — skip if entity has no rowversion yet (newly seeded/mock)
        if (entity.RowVersion.Length > 0 && request.RowVersion.Length > 0 && !entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["تم تعديل البيانات من قبل مستخدم آخر، يرجى التحديث والمحاولة مرة أخرى."]);

        var trimmedName = request.Name.Trim();
        var trimmedDesc = request.Description?.Trim();
        var isTypeChanged = entity.Type != request.Type || entity.NormalBalance != request.NormalBalance;
        var isParentChanged = entity.ParentId != request.ParentId;

        // Matrix
        if (!AccountGroupHierarchyHelper.IsNormalBalanceCompatible(request.Type, request.NormalBalance))
            return Result.Failure(["الرصيد الطبيعي غير متوافق مع النوع."]);

        // Parent handling — only validate if parent changed
        byte newLevel = entity.Level;
        if (isParentChanged)
        {
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                    return Result.Failure(["لا يمكن أن تكون المجموعة أباً لنفسها."]);

                var newParent = await context.AccountGroups.FindAsync(request.ParentId.Value, cancellationToken);
                if (newParent == null)
                    return Result.Failure(["المجموعة الأب غير موجودة."]);
                if (!newParent.IsActive)
                    return Result.Failure(["لا يمكن نقل مجموعة تحت أب معطل."]);
                if (!AccountGroupHierarchyHelper.IsTypeInheritanceValid(request.Type, newParent.Type))
                    return Result.Failure(["نوع الابن يجب أن يساوي نوع الأب."]);
                if (await AccountGroupHierarchyHelper.WouldCreateCycleAsync(context, request.Id, request.ParentId, cancellationToken))
                    return Result.Failure(["لا يمكن نقل مجموعة تحت أحد أبنائها (حلقة هرمية)."]);

                newLevel = AccountGroupHierarchyHelper.ComputeLevel(newParent);
                if (AccountGroupHierarchyHelper.ExceedsMaxDepth(newLevel))
                    return Result.Failure(["تجاوز الحد الأقصى للمستويات (5)."]);

                var subtreeIds = await AccountGroupHierarchyHelper.GetSubtreeIdsAsync(context, request.Id, cancellationToken);
                var maxLevelInSubtree = await AccountGroupHierarchyHelper.GetMaxLevelInSubtreeAsync(context, subtreeIds, cancellationToken);
                var relativeDepth = (byte)(maxLevelInSubtree - entity.Level);
                if (newLevel + relativeDepth > AccountGroupHierarchyHelper.MaxLevel)
                    return Result.Failure(["تجاوز الحد الأقصى للمستويات (5) للشجرة الفرعية."]);
            }
            else
            {
                newLevel = 1;
                var subtreeIds = await AccountGroupHierarchyHelper.GetSubtreeIdsAsync(context, request.Id, cancellationToken);
                var maxLevelInSubtree = await AccountGroupHierarchyHelper.GetMaxLevelInSubtreeAsync(context, subtreeIds, cancellationToken);
                var relativeDepth = (byte)(maxLevelInSubtree - entity.Level);
                if (newLevel + relativeDepth > AccountGroupHierarchyHelper.MaxLevel)
                    return Result.Failure(["تجاوز الحد الأقصى للمستويات (5) للشجرة الفرعية."]);
            }
        }

        // Posted-entries guard if Type/NormalBalance changed
        if (isTypeChanged)
        {
            var subtreeIds = await AccountGroupHierarchyHelper.GetSubtreeIdsAsync(context, request.Id, cancellationToken);
            if (await AccountGroupGuardHelper.HasPostedEntriesAsync(context, subtreeIds, cancellationToken))
                return Result.Failure(["لا يمكن تغيير النوع/الرصيد لمجموعة مرتبطة بقيود مرحلة."]);
        }

        entity.Name = trimmedName;
        entity.Type = request.Type;
        entity.NormalBalance = request.NormalBalance;
        entity.Description = string.IsNullOrWhiteSpace(trimmedDesc) ? null : trimmedDesc;
        entity.ParentId = request.ParentId;

        // Recalculate Level for subtree if parent changed
        if (isParentChanged)
        {
            entity.Level = newLevel;
            var queue = new Queue<(int Id, byte Level)>();
            queue.Enqueue((entity.Id, newLevel));
            var visited = new HashSet<int> { entity.Id };
            while (queue.Count > 0)
            {
                var (pid, plevel) = queue.Dequeue();
                var kids = await context.AccountGroups.Where(x => x.ParentId == pid).ToListAsync(cancellationToken);
                foreach (var kid in kids)
                {
                    if (!visited.Add(kid.Id)) continue;
                    kid.Level = (byte)(plevel + 1);
                    queue.Enqueue((kid.Id, kid.Level));
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateAccountGroupCommandValidator : AbstractValidator<UpdateAccountGroupCommand>
{
    public UpdateAccountGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("الاسم مطلوب.")
            .MaximumLength(200).WithMessage("الاسم يجب ألا يتجاوز 200 حرفاً.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع المجموعة غير صالح.");

        RuleFor(x => x.NormalBalance)
            .IsInEnum().WithMessage("الرصيد الطبيعي غير صالح.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب ألا يتجاوز 500 حرفاً.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion مطلوب للتحكم في التزامن.");

        RuleFor(x => x.ParentId)
            .GreaterThan(0).When(x => x.ParentId.HasValue).WithMessage("معرف الأب غير صالح.");
    }
}
