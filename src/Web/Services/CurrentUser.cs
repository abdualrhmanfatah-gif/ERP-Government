using System.Security.Claims;

using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? Id
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

    public List<string>? Permissions => _httpContextAccessor.HttpContext?.User?.FindAll("permission").Select(x => x.Value).ToList();
}
