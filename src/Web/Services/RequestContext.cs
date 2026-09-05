using ERP_Government.Application.Common.Interfaces;

namespace ERP_Government.Web.Services;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Endpoint => _httpContextAccessor.HttpContext?.Request?.Path.Value;
    public string? Method => _httpContextAccessor.HttpContext?.Request?.Method;
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}
