using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemCategories.Commands.UpdateItemCategory;

[Authorize(Policy = PermissionCodes.ItemCategoriesUpdate)]
public record UpdateItemCategoryCommand(
    int Id,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? ParentItemCategoryId,
    int? ExpenseAccountId,
    int? InventoryAccountId,
    int? TaxAccountId,
    string? TaxClass,
    bool IsActive) : IRequest<Result>;

public class UpdateItemCategoryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateItemCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateItemCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ItemCategories.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Category not found."]);

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.NameEn = request.NameEn;
        entity.Description = request.Description;
        entity.ParentItemCategoryId = request.ParentItemCategoryId;
        entity.ExpenseAccountId = request.ExpenseAccountId;
        entity.InventoryAccountId = request.InventoryAccountId;
        entity.TaxAccountId = request.TaxAccountId;
        entity.TaxClass = request.TaxClass;
        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        if (request.ParentItemCategoryId.HasValue)
        {
            var parent = await context.ItemCategories.FindAsync(request.ParentItemCategoryId.Value, cancellationToken);
            if (parent is not null)
            {
                entity.Level = (parent.Level ?? 0) + 1;
                entity.Breadcrumb = string.IsNullOrEmpty(parent.Breadcrumb)
                    ? parent.Code
                    : $"{parent.Breadcrumb}/{parent.Code}";
            }
        }
        else
        {
            entity.Level = null;
            entity.Breadcrumb = null;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateItemCategoryCommandValidator : AbstractValidator<UpdateItemCategoryCommand>
{
    public UpdateItemCategoryCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid category.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (x, code, ct) =>
                !await context.ItemCategories.AnyAsync(c => c.Code == code && c.Id != x.Id, ct))
            .WithMessage("A category with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x)
            .Must(x => !x.ParentItemCategoryId.HasValue || x.ParentItemCategoryId != x.Id)
            .WithMessage("A category cannot be its own parent.");
    }
}
