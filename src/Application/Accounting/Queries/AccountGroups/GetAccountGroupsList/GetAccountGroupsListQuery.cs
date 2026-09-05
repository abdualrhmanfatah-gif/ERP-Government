using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Queries.AccountGroups.GetAccountGroupsList;

[Authorize(Policy = PermissionCodes.ChartOfAccountsRead)]
public class GetAccountGroupsListQuery : IRequest<PaginatedAccountGroupsResponse>
{
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public AccountGroupType? Type { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAccountGroupsListQueryHandler(
    IApplicationDbContext context,
    IMapper mapper) : IRequestHandler<GetAccountGroupsListQuery, PaginatedAccountGroupsResponse>
{
    public async Task<PaginatedAccountGroupsResponse> Handle(
        GetAccountGroupsListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
        var search = request.Search?.Trim();
        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var hasFilters = request.Type.HasValue || request.IsActive.HasValue || hasSearch;

        // Flat mode when any search/filter present
        if (hasFilters)
        {
            var query = context.AccountGroups.AsQueryable();

            if (hasSearch)
            {
                var normalized = search!.ToUpperInvariant();
                query = query.Where(x => x.Code.ToUpper().Contains(normalized) || x.Name.ToUpper().Contains(normalized));
            }

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            if (request.Type.HasValue)
                query = query.Where(x => x.Type == request.Type.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = mapper.Map<List<AccountGroupDto>>(items);
            // Build ancestorPath for each item (max 5 steps)
            var resultItems = new List<AccountGroupDto>();
            foreach (var dto in dtos)
            {
                var original = items.First(i => i.Id == dto.Id);
                var path = await AccountGroupHierarchyHelper.BuildAncestorPathAsync(context, original.ParentId, cancellationToken);
                resultItems.Add(new AccountGroupDto
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    Type = dto.Type,
                    NormalBalance = dto.NormalBalance,
                    Description = dto.Description,
                    ParentId = dto.ParentId,
                    Level = dto.Level,
                    IsActive = dto.IsActive,
                    RowVersion = dto.RowVersion,
                    AncestorPath = path
                });
            }

            return new PaginatedAccountGroupsResponse
            {
                Mode = "flat",
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = resultItems
            };
        }

        // Tree mode: paginate roots but include full subtree so level 2..5 visible
        var rootsQuery = context.AccountGroups.Where(x => x.ParentId == null).OrderBy(x => x.Code);
        var rootTotal = await rootsQuery.CountAsync(cancellationToken);
        var rootTotalPages = (int)Math.Ceiling(rootTotal / (double)pageSize);
        var roots = await rootsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        if (roots.Count == 0)
        {
            return new PaginatedAccountGroupsResponse
            {
                Mode = "tree",
                Page = page,
                PageSize = pageSize,
                TotalCount = rootTotal,
                TotalPages = rootTotalPages,
                Items = []
            };
        }

        var rootIds = roots.Select(r => r.Id).ToList();
        // BFS collect descendants of paginated roots
        var allDescendants = new List<Domain.Accounting.Entities.AccountGroup>();
        var queue = new Queue<int>(rootIds);
        var visited = new HashSet<int>(rootIds);
        while (queue.Count > 0)
        {
            var pid = queue.Dequeue();
            var children = await context.AccountGroups.Where(x => x.ParentId == pid).OrderBy(x => x.Code).ToListAsync(cancellationToken);
            foreach (var c in children)
            {
                if (visited.Add(c.Id))
                {
                    allDescendants.Add(c);
                    queue.Enqueue(c.Id);
                }
            }
        }

        var combined = roots.Concat(allDescendants).ToList();
        // Build lookup parentId -> children sorted by Code (use -1 sentinel for null)
        var lookup = combined.GroupBy(x => x.ParentId ?? -1).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Code).ToList());
        var orderedEntities = new List<Domain.Accounting.Entities.AccountGroup>();
        void Dfs(int parentId)
        {
            if (!lookup.TryGetValue(parentId, out var children)) return;
            foreach (var child in children)
            {
                orderedEntities.Add(child);
                Dfs(child.Id);
            }
        }
        var sortedRoots = roots.OrderBy(r => r.Code).ToList();
        foreach (var r in sortedRoots)
        {
            orderedEntities.Add(r);
            Dfs(r.Id);
        }

        var rootDtos2 = mapper.Map<List<AccountGroupDto>>(orderedEntities);

        return new PaginatedAccountGroupsResponse
        {
            Mode = "tree",
            Page = page,
            PageSize = pageSize,
            TotalCount = rootTotal,
            TotalPages = rootTotalPages,
            Items = rootDtos2
        };
    }
}
