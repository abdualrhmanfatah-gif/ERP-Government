using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Accounting.Queries.Templates.GetTemplateById;

[Authorize(Policy = PermissionCodes.TemplatesRead)]
public class GetTemplateByIdQuery : IRequest<JournalEntryTemplateDto?>
{
    public int Id { get; init; }
}

public class GetTemplateByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetTemplateByIdQuery, JournalEntryTemplateDto?>
{
    public async Task<JournalEntryTemplateDto?> Handle(
        GetTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.JournalEntryTemplates
            .Include(x => x.Journal)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return null;

        return mapper.Map<JournalEntryTemplateDto>(entity);
    }
}
