namespace ERP_Government.Web.AcceptanceTests.Pages;

public class DisposalPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-disposals";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SelectAsset(string assetCode)
        => Page.Locator("[name='assetId']").ClickAsync();

    public Task SetDisposalMethod(string method)
        => Page.SelectOptionAsync("[name='disposalMethod']", method);

    public Task SetSaleProceeds(string amount)
        => Page.FillAsync("[name='saleProceeds']", amount);

    public Task ClickPost() => Page.Locator("button:has-text('ترحيل')").ClickAsync();

    public async Task<string> GetAssetStatus()
    {
        var text = await Page.Locator("[data-testid='asset-status']").TextContentAsync();
        return text ?? string.Empty;
    }
}
