using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.TemplateLines.GetTemplateLines;

[Authorize(Policy = PermissionCodes.TemplatesRead)]
public class GetTemplateLinesQuery : IRequest<List<JournalEntryTemplateLineDto>>
{
    public int TemplateId { get; init; }
}

public class GetTemplateLinesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetTemplateLinesQuery, List<JournalEntryTemplateLineDto>>
{
    public async Task<List<JournalEntryTemplateLineDto>> Handle(
        GetTemplateLinesQuery request,
        CancellationToken cancellationToken)
    {
        var lines = await context.JournalEntryTemplateLines
            .Include(x => x.Account)
            .Include(x => x.CostCenter)
            .Where(x => x.TemplateId == request.TemplateId)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<JournalEntryTemplateLineDto>>(lines);
    }
}
