using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Budgeting.Queries.BudgetClassifications;

[Authorize(Policy = PermissionCodes.BudgetClassificationsView)]
public record GetBudgetClassificationsTreeQuery : IRequest<List<BudgetClassificationDto>>;

public class GetBudgetClassificationsTreeQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetBudgetClassificationsTreeQuery, List<BudgetClassificationDto>>
{
    public async Task<List<BudgetClassificationDto>> Handle(
        GetBudgetClassificationsTreeQuery request,
        CancellationToken cancellationToken)
    {
        var allItems = await context.BudgetClassifications
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<List<BudgetClassificationDto>>(allItems);
        var lookup = dtos.ToDictionary(x => x.Id);

        List<BudgetClassificationDto> roots = [];

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && lookup.TryGetValue(dto.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        SetLevel(roots, 1);

        return roots;
    }

    private static void SetLevel(List<BudgetClassificationDto> items, int level)
    {
        foreach (var item in items)
        {
            item.Level = level;
            if (item.Children.Count > 0)
                SetLevel(item.Children, level + 1);
        }
    }
}
