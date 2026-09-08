using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.SendToTreasury;

[Authorize(Policy = PermissionCodes.PaymentOrdersSendToTreasury)]
public class SendToTreasuryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string TreasuryReference { get; init; } = string.Empty;
    public byte[] RowVersion { get; init; } = [];
}

public class SendToTreasuryCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IUser user) : IRequestHandler<SendToTreasuryCommand, Result>
{
    public async Task<Result> Handle(
        SendToTreasuryCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Approved)
            return Result.Failure(["Only approved payment orders can be sent to treasury."]);

        entity.Status = PaymentOrderStatus.SentToTreasury;
        entity.TreasuryStatus = "Sent";
        entity.TreasuryReference = request.TreasuryReference;
        entity.TreasurySentAt = DateTimeOffset.UtcNow;

        await statusLogger.LogAsync(
            "paymentorders",
            entity.Id,
            PaymentOrderStatus.Approved.ToString(),
            PaymentOrderStatus.SentToTreasury.ToString(),
            userId,
            $"Treasury reference: {request.TreasuryReference}",
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class SendToTreasuryCommandValidator : AbstractValidator<SendToTreasuryCommand>
{
    public SendToTreasuryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");

        RuleFor(x => x.TreasuryReference)
            .NotEmpty().WithMessage("Treasury reference is required.");
    }
}
