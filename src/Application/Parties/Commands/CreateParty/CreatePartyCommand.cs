using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;

namespace ERP_Government.Application.Parties.Commands.CreateParty;

[Authorize(Policy = PermissionCodes.PartiesCreate)]
public record CreatePartyCommand(
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<Result<int>>;

public class CreatePartyCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService) : IRequestHandler<CreatePartyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreatePartyCommand request,
        CancellationToken cancellationToken)
    {
        var partyCode = await sequenceService.GenerateNextNumberAsync("Party", cancellationToken);
        if (partyCode.StartsWith("Error:"))
            return Result<int>.Failure([partyCode]);

        var entity = new Party
        {
            PartyCode = partyCode,
            PartyType = request.PartyType,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            TaxNumber = request.TaxNumber,
            NationalId = request.NationalId,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Notes = request.Notes,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Parties.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreatePartyCommandValidator : AbstractValidator<CreatePartyCommand>
{
    public CreatePartyCommandValidator()
    {
        RuleFor(x => x.PartyType)
            .IsInEnum().WithMessage("Invalid party type.");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name (Arabic) is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.NameEn)
            .MaximumLength(500).WithMessage("Name (English) must not exceed 500 characters.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number must not exceed 50 characters.");

        RuleFor(x => x.NationalId)
            .MaximumLength(50).WithMessage("National ID must not exceed 50 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .EmailAddress().WithMessage("Invalid email address.");
    }
}
