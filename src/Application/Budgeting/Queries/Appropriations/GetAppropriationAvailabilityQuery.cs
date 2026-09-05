using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Budgeting.Queries.Appropriations;

[Authorize(Policy = PermissionCodes.AppropriationsView)]
public record GetAppropriationAvailabilityQuery(int Id) : IRequest<BudgetAvailabilitySummary?>;

public class GetAppropriationAvailabilityQueryHandler(
    IApplicationDbContext context,
    IBudgetAvailabilityService availabilityService) : IRequestHandler<GetAppropriationAvailabilityQuery, BudgetAvailabilitySummary?>
{
    public async Task<BudgetAvailabilitySummary?> Handle(
        GetAppropriationAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var appropriation = await context.Appropriations
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (appropriation is null)
            return null;

        return await availabilityService.GetAvailabilitySummaryAsync(appropriation.BudgetItemId);
    }
}
