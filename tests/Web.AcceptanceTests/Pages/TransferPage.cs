namespace ERP_Government.Web.AcceptanceTests.Pages;

public class TransferPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-transfers";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SelectAsset(string assetCode)
        => Page.Locator("[name='assetId']").ClickAsync();

    public Task SetNewLocation()
        => Page.Locator("[name='toLocationId']").ClickAsync();

    public Task SetNewCustodian()
        => Page.Locator("[name='toEmployeeId']").ClickAsync();

    public Task ClickExecute() => Page.Locator("button:has-text('تنفيذ')").ClickAsync();

    public Task ClickSave() => Page.Locator("button[type='submit']").ClickAsync();

    public async Task<string> GetTransferStatus()
    {
        var text = await Page.Locator("[data-testid='transfer-status']").TextContentAsync();
        return text ?? string.Empty;
    }
}
