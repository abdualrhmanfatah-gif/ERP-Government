using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Commands;

[Authorize(Policy = PermissionCodes.AssetCountsCreate)]
public record CreateAssetPhysicalCountCommand(
    DateOnly CountDate,
    string CountType,
    int? LocationId,
    int? DepartmentId,
    string? Notes) : IRequest<Result<int>>;

public class CreateAssetPhysicalCountCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreateAssetPhysicalCountCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetPhysicalCountCommand request, CancellationToken ct)
    {
        string number;
        try
        {
            number = await sequenceService.GenerateNextNumberAsync("AssetPhysicalCount", ct);
        }
        catch (Exception)
        {
            return Result<int>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل في توليد رقم الجرد");
        }

        string scopeLabel = (request.LocationId, request.DepartmentId) switch
        {
            (null, null) => "جميع المواقع — جميع الإدارات",
            (not null, null) => "موقع محدد — جميع الإدارات",
            (null, not null) => "جميع المواقع — إدارة محددة",
            (not null, not null) => "موقع محدد — إدارة محددة"
        };

        var count = new AssetPhysicalCount
        {
            CountNumber = number,
            CountDate = request.CountDate,
            LocationId = request.LocationId,
            DepartmentId = request.DepartmentId,
            ResolvedScopeLabel = scopeLabel,
            CountType = request.CountType,
            Status = CountStatus.Draft,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetPhysicalCounts.Add(count);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(count.Id);
    }
}

public class CreateAssetPhysicalCountCommandValidator : AbstractValidator<CreateAssetPhysicalCountCommand>
{
    public CreateAssetPhysicalCountCommandValidator()
    {
        RuleFor(x => x.CountDate)
            .NotEmpty().WithMessage("تاريخ الجرد مطلوب");

        RuleFor(x => x.CountType)
            .NotEmpty().WithMessage("نوع الجرد مطلوب");
    }
}
