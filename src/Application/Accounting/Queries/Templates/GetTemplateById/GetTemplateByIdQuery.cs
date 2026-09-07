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

        var dto = mapper.Map<JournalEntryTemplateDto>(entity);

        var lines = await context.JournalEntryTemplateLines
            .Include(x => x.Account)
            .Include(x => x.CostCenter)
            .Where(x => x.TemplateId == request.Id)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        var lineDtos = mapper.Map<List<JournalEntryTemplateLineDto>>(lines);

        dto.Lines = lineDtos;
        dto.TotalDebit = lineDtos.Sum(x => x.Debit);
        dto.TotalCredit = lineDtos.Sum(x => x.Credit);

        return dto;
    }
}
