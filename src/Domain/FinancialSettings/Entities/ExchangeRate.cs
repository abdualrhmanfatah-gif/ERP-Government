using ERP_Government.Domain.Common;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Domain.FinancialSettings.Entities;

public class ExchangeRate : BaseAuditableEntity
{
    public int BaseCurrencyId { get; set; }
    public Currency BaseCurrency { get; set; } = null!;
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public DateOnly RateDate { get; set; }
    public ExchangeRateType RateType { get; set; }
    public decimal Rate { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}
