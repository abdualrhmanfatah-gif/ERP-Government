using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.FinancialSettings.Common.Services;

public class DocumentSequenceService : IDocumentSequenceService
{
    private readonly IApplicationDbContext _context;
    private readonly IDatabaseTransactionFactory _transactionFactory;

    private static readonly Dictionary<string, string> PrefixMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Budget"] = "BGT",
        ["Appropriation"] = "APR",
        ["Encumbrance"] = "ENC",
        ["PaymentOrder"] = "PO",
        ["PaymentExecution"] = "PE",
        ["AdvancePayment"] = "ADV",
        ["JournalEntry"] = "JRN",
        ["PurchaseRequest"] = "PRQ",
        ["RequestForQuotation"] = "RFQ",
        ["Quotation"] = "QT",
        ["PurchaseOrder"] = "PO",
        ["RevenueReceipt"] = "REV",
        ["GoodsReceiptNote"] = "GRN",
        ["StockTake"] = "STK",
        ["Asset"] = "AST",
        ["AssetDisposal"] = "DSP",
        ["AssetRevaluation"] = "REV",
        ["AssetImpairment"] = "IMP",
        ["Party"] = "PTY",
        ["ReceiptVoucher"] = "RCV",
        ["DepositSlip"] = "DSL",
        ["DisbursementRequest"] = "DSB",
        ["Payment"] = "PAY"
    };

    public DocumentSequenceService(IApplicationDbContext context, IDatabaseTransactionFactory transactionFactory)
    {
        _context = context;
        _transactionFactory = transactionFactory;
    }

    public async Task<string> GenerateNextNumberAsync(string documentType, CancellationToken cancellationToken = default)
    {
        if (!PrefixMap.TryGetValue(documentType, out var prefix))
            throw new DocumentSequenceException($"Unknown document type '{documentType}'.");

        // Atomic: read + version-checked increment inside a transaction.
        // Two concurrent callers with the same read rowversion — only one update succeeds.
        await using var transaction = await _transactionFactory
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var row = await _context.DocumentSequences
                .Where(s => s.DocumentType == documentType)
                .FirstOrDefaultAsync(cancellationToken);

            if (row is null)
                throw new DocumentSequenceException($"No sequence found for document type '{documentType}'.");

            if (!row.IsActive)
                throw new DocumentSequenceException($"Sequence for '{documentType}' is deactivated.");

            var oldVersion = (byte[])row.RowVersion.Clone();
            row.CurrentNumber++;

            var affected = await _context.SaveChangesAsync(cancellationToken);

            if (affected == 0)
                throw new SequenceConcurrencyException(documentType);

            await transaction.CommitAsync(cancellationToken);

            return $"{prefix}-{row.CurrentNumber:D6}";
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
