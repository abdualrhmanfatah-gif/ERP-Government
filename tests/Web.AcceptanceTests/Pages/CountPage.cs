namespace ERP_Government.Web.AcceptanceTests.Pages;

public class CountPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-counts";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task CreateEntityWideCount()
    {
        // Leave location and department empty for entity-wide count
        return Page.Locator("button[type='submit']").ClickAsync();
    }

    public Task ClickStart() => Page.Locator("button:has-text('بدء')").ClickAsync();

    public Task ClickComplete() => Page.Locator("button:has-text('إكمال')").ClickAsync();

    public Task ClickReview() => Page.Locator("button:has-text('مراجعة')").ClickAsync();

    public async Task<string> GetScopeLabel()
    {
        var text = await Page.Locator("[data-testid='scope-label']").TextContentAsync();
        return text ?? string.Empty;
    }

    public async Task<string> GetCountStatus()
    {
        var text = await Page.Locator("[data-testid='count-status']").TextContentAsync();
        return text ?? string.Empty;
    }

    public Task RecordObservation(int rowIndex, string foundState)
        => Page.SelectOptionAsync($"[data-testid='line-{rowIndex}-isFound']", foundState);

    public Task<bool> HasFrozenSystemFields() =>
        Page.Locator("[data-testid='system-location']").IsVisibleAsync();
}
