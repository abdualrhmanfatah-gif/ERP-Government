using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.Templates.GetTemplatesList;

[Authorize(Policy = PermissionCodes.TemplatesRead)]
public class GetTemplatesListQuery : IRequest<List<JournalEntryTemplateDto>>
{
    public bool? IsActive { get; init; }
    public int? JournalId { get; init; }
    public JournalEntryTemplateType? TemplateType { get; init; }
}

public class GetTemplatesListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetTemplatesListQuery, List<JournalEntryTemplateDto>>
{
    public async Task<List<JournalEntryTemplateDto>> Handle(
        GetTemplatesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.JournalEntryTemplates
            .Include(x => x.Journal)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.JournalId.HasValue)
            query = query.Where(x => x.JournalId == request.JournalId.Value);

        if (request.TemplateType.HasValue)
            query = query.Where(x => x.TemplateType == request.TemplateType.Value);

        var items = await query
            .OrderBy(x => x.TemplateName)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<JournalEntryTemplateDto>>(items);
    }
}
