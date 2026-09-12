using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.BudgetItemAllocations;

[Authorize(Policy = PermissionCodes.BudgetItemAllocationsUpdate)]
public record UpdateBudgetItemAllocationCommand(
    int Id,
    decimal ProposedAmount,
    string? Remarks,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateBudgetItemAllocationCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateBudgetItemAllocationCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBudgetItemAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.BudgetItemAllocations
            .Include(x => x.Budget)
            .Include(x => x.BudgetItem)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["المخصص غير موجود"]);

        if (entity.Budget.Status != BudgetStatus.Draft)
            return Result.Failure(["لا يمكن تعديل المخصص إلا في الموازنة المسودة"]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["تعارض في البيانات. تم تعديل السجل من مستخدم آخر."]);

        entity.ProposedAmount = request.ProposedAmount;
        entity.Remarks = request.Remarks;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateBudgetItemAllocationCommandValidator : AbstractValidator<UpdateBudgetItemAllocationCommand>
{
    public UpdateBudgetItemAllocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Allocation ID must be greater than 0.");

        RuleFor(x => x.ProposedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("المبلغ المقترح لا يمكن أن يكون سالبًا");
    }
}
