namespace ERP_Government.Web.AcceptanceTests.Pages;

public class DepreciationPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/assets/depreciation";

    public Task ClickRun() => Page.Locator("button:has-text('تشغيل')").ClickAsync();

    public Task SelectPeriod(string period) => Page.SelectOptionAsync("[name='fiscalPeriodId']", period);

    public Task ClickPost() => Page.Locator("button:has-text('ترحيل')").ClickAsync();

    public Task ClickReverse() => Page.Locator("button:has-text('عكس')").ClickAsync();

    public Task SetReversalReason(string reason)
        => Page.FillAsync("[name='reversalReason']", reason);

    public Task ClickSave() => Page.Locator("button[type='submit']").ClickAsync();

    public async Task<string> GetScheduleStatus()
    {
        var text = await Page.Locator("[data-testid='schedule-status']").TextContentAsync();
        return text ?? string.Empty;
    }

    public Task<bool> HasJournalEntryLink() =>
        Page.Locator("[data-testid='journal-entry-link']").IsVisibleAsync();
}
