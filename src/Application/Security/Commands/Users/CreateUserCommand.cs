using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

using FluentValidation;

using MediatR;

namespace ERP_Government.Application.Security.Commands.Users;

// T008 — CreateUserCommand
[Authorize(Policy = PermissionCodes.UsersCreate)]
public class CreateUserCommand : IRequest<Result<int>>
{
    public string Login { get; init; } = string.Empty;
    public AccountType AccountType { get; init; }
    public int? DepartmentId { get; init; }
}

public class CreateUserCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateUserCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var loginExists = await context.Users
            .AnyAsync(u => u.Login == request.Login, cancellationToken);

        if (loginExists)
            return Result<int>.Failure(["Login already exists."]);

        var entity = new User
        {
            Login = request.Login,
            AccountType = request.AccountType,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            FailedLoginAttempts = 0,
            MfaEnabled = false,
            MustChangePassword = false
        };

        context.Users.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Login is required.")
            .MaximumLength(100).WithMessage("Login must not exceed 100 characters.");

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Invalid account type.");
    }
}
