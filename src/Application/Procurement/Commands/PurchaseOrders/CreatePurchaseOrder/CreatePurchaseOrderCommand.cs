using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using FluentValidation;

namespace ERP_Government.Application.Procurement.Commands.PurchaseOrders.CreatePurchaseOrder;

[Authorize(Policy = PermissionCodes.PurchaseOrdersCreate)]
public record CreatePurchaseOrderCommand(
    int? PurchaseRequestId,
    int? QuotationId,
    int SupplierPartyId,
    int? WarehouseId,
    int? DeliveryLocationId,
    string? CurrencyCode,
    decimal? ExchangeRate,
    string? PaymentTerms,
    string? DeliveryTerms,
    DateTime? ExpectedDeliveryDate,
    string? Notes,
    List<PurchaseOrderLineDto> Lines) : IRequest<Result<int>>;

public record PurchaseOrderLineDto(
    int? Id,
    int PurchaseRequestDetailId,
    int? QuotationDetailId,
    int ItemId,
    int UnitId,
    decimal OrderedQuantity,
    decimal UnitPrice,
    decimal? DiscountPercent,
    decimal? TaxPercent,
    DateTime? ExpectedDeliveryDate,
    string? Notes);

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierPartyId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.Lines)
            .NotNull().WithMessage("At least one line is required.")
            .NotEmpty().WithMessage("At least one line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.OrderedQuantity)
                .GreaterThan(0).WithMessage("Ordered quantity must be greater than 0.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater.");

            line.RuleFor(l => l.DiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100.")
                .When(l => l.DiscountPercent.HasValue);

            line.RuleFor(l => l.TaxPercent)
                .InclusiveBetween(0, 100).WithMessage("Tax percent must be between 0 and 100.")
                .When(l => l.TaxPercent.HasValue);
        });
    }
}
