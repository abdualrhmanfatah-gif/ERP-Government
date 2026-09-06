using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Parties.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Parties.Commands.UpdateParty;

[Authorize(Policy = PermissionCodes.PartiesUpdate)]
public record UpdatePartyCommand(
    int Id,
    PartyType PartyType,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? NationalId,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes) : IRequest<Result>;

public class UpdatePartyCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdatePartyCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePartyCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Parties.FindAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(["Party not found."]);

        entity.PartyType = request.PartyType;
        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.TaxNumber = request.TaxNumber;
        entity.NationalId = request.NationalId;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.Notes = request.Notes;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdatePartyCommandValidator : AbstractValidator<UpdatePartyCommand>
{
    public UpdatePartyCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid party ID.");

        RuleFor(x => x.PartyType)
            .IsInEnum().WithMessage("Invalid party type.");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name (Arabic) is required.")
            .MaximumLength(500).WithMessage("Name must not exceed 500 characters.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage("Tax number must not exceed 50 characters.")
            .MustAsync(async (model, taxNumber, ct) =>
                string.IsNullOrEmpty(taxNumber) ||
                !await context.Parties.AnyAsync(p => p.TaxNumber == taxNumber && p.Id != model.Id, ct))
            .WithMessage("A party with this tax number already exists.");

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .EmailAddress().WithMessage("Invalid email address.");
    }
}
