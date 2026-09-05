using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Enums;

using FluentValidation;

using MediatR;

namespace ERP_Government.Application.Security.Commands.Users;

// T009 — UpdateUserCommand
[Authorize(Policy = PermissionCodes.UsersUpdate)]
public class UpdateUserCommand : IRequest<Result>
{
    public int Id { get; init; }
    public AccountType AccountType { get; init; }
    public int? DepartmentId { get; init; }
    public string? MfaMethod { get; init; }
}

public class UpdateUserCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["User not found."]);

        entity.AccountType = request.AccountType;
        entity.DepartmentId = request.DepartmentId;
        entity.MfaMethod = request.MfaMethod;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid user ID.");

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Invalid account type.");
    }
}
