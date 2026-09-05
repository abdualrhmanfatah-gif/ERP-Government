using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.Commands.Users;

// T026 — ImportUsersCommand (batch CSV import)
[Authorize(Policy = PermissionCodes.UsersCreate)]
public class ImportUsersCommand : IRequest<Result<ImportUsersResult>>
{
    public List<ImportUserDto> Users { get; init; } = [];
}

public class ImportUserDto
{
    public string Login { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public int? DepartmentId { get; init; }
}

public class ImportUsersResult
{
    public int TotalProcessed { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<ImportError> Errors { get; set; } = [];
}

public class ImportError
{
    public int Row { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class ImportUsersCommandHandler(
    IApplicationDbContext context) : IRequestHandler<ImportUsersCommand, Result<ImportUsersResult>>
{
    public async Task<Result<ImportUsersResult>> Handle(
        ImportUsersCommand request,
        CancellationToken cancellationToken)
    {
        var result = new ImportUsersResult
        {
            TotalProcessed = request.Users.Count
        };

        foreach (var userDto in request.Users)
        {
            try
            {
                // Validate account type
                if (!Enum.TryParse<Domain.Security.Enums.AccountType>(userDto.AccountType, true, out var accountType))
                {
                    result.Errors.Add(new ImportError
                    {
                        Row = result.TotalProcessed - request.Users.Count + 1,
                        Login = userDto.Login,
                        Error = $"Invalid account type: {userDto.AccountType}"
                    });
                    result.FailedImports++;
                    continue;
                }

                // Check for duplicate login
                var exists = await context.Users.AnyAsync(u => u.Login == userDto.Login, cancellationToken);
                if (exists)
                {
                    result.Errors.Add(new ImportError
                    {
                        Row = result.TotalProcessed - request.Users.Count + 1,
                        Login = userDto.Login,
                        Error = "Login already exists"
                    });
                    result.FailedImports++;
                    continue;
                }

                var entity = new Domain.Security.Entities.User
                {
                    Login = userDto.Login,
                    AccountType = accountType,
                    DepartmentId = userDto.DepartmentId,
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    MfaEnabled = false,
                    MustChangePassword = true
                };

                context.Users.Add(entity);
                result.SuccessfulImports++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError
                {
                    Row = result.TotalProcessed - request.Users.Count + 1,
                    Login = userDto.Login,
                    Error = ex.Message
                });
                result.FailedImports++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<ImportUsersResult>.Success(result);
    }
}

public class ImportUsersCommandValidator : AbstractValidator<ImportUsersCommand>
{
    public ImportUsersCommandValidator()
    {
        RuleFor(x => x.Users)
            .NotEmpty().WithMessage("Users list cannot be empty.");

        RuleForEach(x => x.Users).ChildRules(user =>
        {
            user.RuleFor(u => u.Login)
                .NotEmpty().WithMessage("Login is required.")
                .MaximumLength(100).WithMessage("Login must not exceed 100 characters.");

            user.RuleFor(u => u.AccountType)
                .NotEmpty().WithMessage("Account type is required.");
        });
    }
}
