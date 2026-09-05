using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.AccountingEvents.GetPendingEvents;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class AccountingEvents : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/pending", GetPendingEvents)
            .Produces<List<AccountingEventDto>>()
            .RequireAuthorization(PermissionCodes.AccountingEventsRead);
    }

    [EndpointSummary("Get pending accounting events")]
    public static async Task<List<AccountingEventDto>> GetPendingEvents(
        [FromServices] ISender sender,
        [AsParameters] GetPendingEventsQuery query)
    {
        return await sender.Send(query);
    }
}
