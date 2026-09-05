namespace ERP_Government.Application.Common.Interfaces;

public interface IUser
{
    int? Id { get; }
    List<string>? Roles { get; }
    List<string>? Permissions { get; }
}
