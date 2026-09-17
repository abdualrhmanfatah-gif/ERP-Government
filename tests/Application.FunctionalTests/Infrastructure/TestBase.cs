namespace ERP_Government.Application.FunctionalTests.Infrastructure;

public abstract class TestBase
{
    [SetUp]
    public virtual async Task SetUp()
    {
        await TestApp.ResetState();
    }
}
