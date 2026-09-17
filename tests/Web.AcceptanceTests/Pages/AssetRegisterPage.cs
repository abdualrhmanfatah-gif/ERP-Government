namespace ERP_Government.Web.AcceptanceTests.Pages;

public class AssetRegisterPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/assets";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SelectGroup(string groupCode)
        => Page.Locator("[name='assetGroupId']").ClickAsync();

    public Task SetName(string name) => Page.FillAsync("[name='name']", name);

    public Task SetEmployeeCustodian()
        => Page.Locator("[name='employeeId']").ClickAsync();

    public Task SetAttributeValue(string label, string value)
        => Page.FillAsync($"[data-attribute='{label}']", value);

    public Task ClickActivate() => Page.Locator("button:has-text('تفعيل')").ClickAsync();

    public Task ClickSave() => Page.Locator("button[type='submit']").ClickAsync();

    public async Task<string> GetStatus()
    {
        var text = await Page.Locator("[data-testid='asset-status']").TextContentAsync();
        return text ?? string.Empty;
    }

    public async Task<string> GetDepartment()
    {
        var text = await Page.Locator("[data-testid='asset-department']").TextContentAsync();
        return text ?? string.Empty;
    }
}
