using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.FinancialSettings.Common.Services;

public class DocumentSequenceService : IDocumentSequenceService
{
    private readonly IApplicationDbContext _context;

    private static readonly Dictionary<string, string> PrefixMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Budget"] = "BGT",
        ["Appropriation"] = "APR",
        ["Encumbrance"] = "ENC",
        ["PaymentOrder"] = "PO",
        ["PaymentExecution"] = "PE",
        ["AdvancePayment"] = "ADV",
        ["JournalEntry"] = "JRN",
        ["RecurringEntry"] = "REC",
        ["PurchaseRequest"] = "PRQ",
        ["Quotation"] = "QT",
        ["PurchaseOrder"] = "PO",
        ["ReceiptVoucher"] = "RCV",
        ["DepositSlip"] = "DSL",
        ["GoodsReceiptNote"] = "GRN",
        ["SupplierInvoice"] = "SINV",
        ["StockTake"] = "STK",
        ["Item"] = "ITM",
        ["Asset"] = "AST",
        ["AssetTransfer"] = "TRF",
        ["AssetDisposal"] = "DSP",
        ["AssetRevaluation"] = "REV",
        ["AssetImpairment"] = "IMP",
        ["DepreciationRun"] = "DEP",
        ["AssetPhysicalCount"] = "CNT",
        ["Party"] = "PTY",
        ["DisbursementRequest"] = "DSB",
        ["Payment"] = "PAY",
        ["BudgetTransaction"] = "BTR",
        ["RevenueClaim"] = "RCM",
        ["CollectionOrder"] = "CLO",
        ["DepositSlip47"] = "DS47",
        ["DepositSlip48"] = "DS48",
    };

    public DocumentSequenceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextNumberAsync(string documentType, CancellationToken cancellationToken = default)
    {
        if (!PrefixMap.TryGetValue(documentType, out var prefix))
            throw new DocumentSequenceException($"Unknown document type '{documentType}'.");

        // RowVersion-based optimistic concurrency — no manual transaction needed.
        // SaveChangesAsync checks RowVersion; concurrency conflict → DbUpdateConcurrencyException.
        var row = await _context.DocumentSequences
            .Where(s => s.DocumentType == documentType)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            throw new DocumentSequenceException($"No sequence found for document type '{documentType}'.");

        if (!row.IsActive)
            throw new DocumentSequenceException($"Sequence for '{documentType}' is deactivated.");

        row.CurrentNumber++;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new SequenceConcurrencyException(documentType);
        }

        return $"{prefix}-{row.CurrentNumber:D6}";
    }
}

public class DocumentSequenceException : Exception
{
    public DocumentSequenceException(string message) : base(message) { }
}

public class SequenceConcurrencyException : DocumentSequenceException
{
    public SequenceConcurrencyException(string documentType)
        : base($"Concurrency conflict on sequence '{documentType}'. Another caller modified the sequence.") { }
}
