namespace ERP_Government.Web.AcceptanceTests.Pages;

public class RevaluationPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-revaluations";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SelectAsset(string assetCode)
        => Page.Locator("[name='assetId']").ClickAsync();

    public Task SetNewValue(string value)
        => Page.FillAsync("[name='newValue']", value);

    public Task ClickPost() => Page.Locator("button:has-text('ترحيل')").ClickAsync();

    public async Task<string> GetDirection()
    {
        var text = await Page.Locator("[data-testid='revaluation-direction']").TextContentAsync();
        return text ?? string.Empty;
    }
}
