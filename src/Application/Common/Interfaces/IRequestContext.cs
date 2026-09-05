namespace ERP_Government.Application.Common.Interfaces;

/// <summary>
/// Abstraction for HTTP request context. Implemented in Web layer.
/// </summary>
public interface IRequestContext
{
    string? Endpoint { get; }
    string? Method { get; }
    string? IpAddress { get; }
}
