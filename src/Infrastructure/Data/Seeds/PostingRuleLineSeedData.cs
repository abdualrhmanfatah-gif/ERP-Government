using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// PostingRuleLine seed data — account mappings for PaymentOrderExecuted.
/// </summary>
public static class PostingRuleLineSeedData
{
    /// <summary>
    /// Creates PostingRuleLines for a given PostingRule.
    /// Call after PostingRules are saved (IDs available).
    /// </summary>
    public static List<PostingRuleLine> GetLinesForPaymentOrderExecuted(int postingRuleId) =>
    [
        // Line 1: حساب البنك — دائن (خروجMoney من البنك)
        new()
        {
            PostingRuleId = postingRuleId,
            Sequence = 1,
            AccountSource = AccountSource.FixedAccount,
            FixedAccountId = 14, // Code="1821": حسابات جارية محلية (بنك)
            DebitOrCredit = DebitOrCredit.Credit,
            AmountSource = AmountSource.EventAmount,
            IsActive = true
        },
        // Line 2: حساب المصروف — مدين (تسجيل المصروف)
        new()
        {
            PostingRuleId = postingRuleId,
            Sequence = 2,
            AccountSource = AccountSource.FixedAccount,
            FixedAccountId = 89, // Code="32299": مستلزمات خدمية أخرى ومختلفة
            DebitOrCredit = DebitOrCredit.Debit,
            AmountSource = AmountSource.EventAmount,
            IsActive = true
        }
    ];
}
