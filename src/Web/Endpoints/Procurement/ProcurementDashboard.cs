using ERP_Government.Application.Procurement.Queries.ProcurementDashboard.GetProcurementDashboard;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Procurement;

public class ProcurementDashboard : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetDashboard)
            .RequireAuthorization()
            .Produces<ProcurementDashboardResponse>();
    }

    private static async Task<IResult> HandleGetDashboard(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProcurementDashboardQuery(), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
