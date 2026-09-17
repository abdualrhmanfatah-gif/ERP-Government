using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Depreciation.Run.Commands;

[Authorize(Policy = PermissionCodes.AssetDepreciationRun)]
public record DeleteDepreciationRunCommand(int RunId) : IRequest<Result>;

public class DeleteDepreciationRunCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDepreciationRunCommand, Result>
{
    public async Task<Result> Handle(DeleteDepreciationRunCommand request, CancellationToken ct)
    {
        var run = await context.DepreciationRuns
            .Include(r => r.ScheduleLines)
            .FirstOrDefaultAsync(r => r.Id == request.RunId, ct);

        if (run is null)
            return Failure(ErrorCodes.Assets.DepreciationRunNotFound, "عملية الإهلاك غير موجودة");
        if (run.Status != DepreciationRunStatus.Draft)
            return Failure(ErrorCodes.Assets.InvalidStatusTransition, "لا يمكن حذف العملية إلا من حالة مسودة");

        context.DepreciationScheduleLines.RemoveRange(run.ScheduleLines);
        context.DepreciationRuns.Remove(run);
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static Result Failure(string code, string message) =>
        Result.Failure(code, ErrorCategory.Validation, message);
}

public class DeleteDepreciationRunCommandValidator : AbstractValidator<DeleteDepreciationRunCommand>
{
    public DeleteDepreciationRunCommandValidator() => RuleFor(x => x.RunId).GreaterThan(0);
}
