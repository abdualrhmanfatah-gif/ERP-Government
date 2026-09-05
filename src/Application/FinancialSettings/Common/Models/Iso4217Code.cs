namespace ERP_Government.Application.FinancialSettings.Common.Models;

/// <summary>
/// Represents a valid ISO 4217 currency code entry.
/// </summary>
public record Iso4217Code(string Code, string Name, int DecimalPlaces);
