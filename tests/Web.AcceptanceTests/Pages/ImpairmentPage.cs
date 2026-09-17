namespace ERP_Government.Web.AcceptanceTests.Pages;

public class ImpairmentPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-impairments";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SelectAsset(string assetCode)
        => Page.Locator("[name='assetId']").ClickAsync();

    public Task SetRecoverableAmount(string amount)
        => Page.FillAsync("[name='recoverableAmount']", amount);

    public Task ClickPost() => Page.Locator("button:has-text('ترحيل')").ClickAsync();

    public Task ClickReverse() => Page.Locator("button:has-text('عكس')").ClickAsync();

    public Task SetReversalReason(string reason)
        => Page.FillAsync("[name='reversalReason']", reason);

    public Task ClickSave() => Page.Locator("button[type='submit']").ClickAsync();

    public Task<bool> HasReversalLink() =>
        Page.Locator("[data-testid='reversal-link']").IsVisibleAsync();
}
