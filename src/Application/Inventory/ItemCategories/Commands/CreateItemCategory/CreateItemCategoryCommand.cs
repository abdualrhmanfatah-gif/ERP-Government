using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Inventory.Entities;

namespace ERP_Government.Application.Inventory.ItemCategories.Commands.CreateItemCategory;

[Authorize(Policy = PermissionCodes.ItemCategoriesCreate)]
public record CreateItemCategoryCommand(
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? ParentItemCategoryId,
    int? ExpenseAccountId,
    int? InventoryAccountId,
    int? TaxAccountId,
    string? TaxClass,
    bool IsActive = true) : IRequest<Result<int>>;

public class CreateItemCategoryCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateItemCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateItemCategoryCommand request,
        CancellationToken cancellationToken)
    {
        int? level = null;
        string? breadcrumb = null;

        if (request.ParentItemCategoryId.HasValue)
        {
            var parent = await context.ItemCategories.FindAsync(request.ParentItemCategoryId.Value, cancellationToken);
            if (parent is null)
                return Result<int>.Failure(["Parent category not found."]);

            level = (parent.Level ?? 0) + 1;
            breadcrumb = string.IsNullOrEmpty(parent.Breadcrumb)
                ? parent.Code
                : $"{parent.Breadcrumb}/{parent.Code}";
        }

        var entity = new ItemCategory
        {
            Code = request.Code,
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            ParentItemCategoryId = request.ParentItemCategoryId,
            Level = level,
            Breadcrumb = breadcrumb,
            ExpenseAccountId = request.ExpenseAccountId,
            InventoryAccountId = request.InventoryAccountId,
            TaxAccountId = request.TaxAccountId,
            TaxClass = request.TaxClass,
            IsActive = request.IsActive,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.ItemCategories.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
{
    public CreateItemCategoryCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .MustAsync(async (code, ct) =>
                !await context.ItemCategories.AnyAsync(c => c.Code == code, ct))
            .WithMessage("A category with this code already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.ParentItemCategoryId)
            .MustAsync(async (parentId, ct) =>
                !parentId.HasValue ||
                !await context.ItemCategories.AnyAsync(c => c.Id == parentId.Value && c.ParentItemCategoryId == parentId.Value, ct))
            .WithMessage("A category cannot be its own parent.");
    }
}
