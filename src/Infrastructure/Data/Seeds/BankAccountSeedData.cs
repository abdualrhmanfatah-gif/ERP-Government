using ERP_Government.Domain.Payments.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// BankAccount seed data — 2 accounts.
/// </summary>
public static class BankAccountSeedData
{
    public static List<BankAccount> GetBankAccounts() =>
    [
        new() { Name="حساب صندوق التقاعد - بنك اليمن", BankName="بنك اليمن للإعمار والتنمية", AccountNumber="1234567890", Iban="YE123456789012345678901234", CurrencyId=1, FundId=1, IsDefault=true, OpeningBalance=10000000, CurrentBalance=10000000 },
        new() { Name="حساب صندوق الاستثمار - اليمن والكويت", BankName="بنك اليمن والكويت", AccountNumber="0987654321", Iban="YE98765432109876543210987", CurrencyId=1, FundId=2, IsDefault=false, OpeningBalance=5000000, CurrentBalance=5000000 },
    ];
}
