namespace ERP_Government.Web.AcceptanceTests.Pages;

public class AssetGroupPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/asset-groups";

    public Task ClickCreate() => Page.Locator("button:has-text('إضافة')").ClickAsync();

    public Task SetCode(string code) => Page.FillAsync("[name='code']", code);

    public Task SetName(string name) => Page.FillAsync("[name='name']", name);

    public Task SelectDepreciationMethod(string method)
        => Page.SelectOptionAsync("[name='depreciationMethod']", method);

    public Task SetDepreciationRate(string rate)
        => Page.FillAsync("[name='depreciationRate']", rate);

    public Task SetDepreciationAccount()
        => Page.Locator("[name='accumulatedDepreciationAccountId']").ClickAsync();

    public Task ClickSave() => Page.Locator("button[type='submit']").ClickAsync();

    public Task AssertGroupInList(string code)
        => Assertions.Expect(Page.Locator($"td:has-text('{code}')")).ToBeVisibleAsync();
}
