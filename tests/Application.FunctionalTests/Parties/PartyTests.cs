using ERP_Government.Application.Parties.Commands.CreateParty;
using ERP_Government.Application.Parties.Commands.TogglePartyActive;
using ERP_Government.Application.Parties.Commands.UpdateParty;
using ERP_Government.Application.Parties.Queries.GetParties;
using ERP_Government.Application.Parties.Queries.GetPartyById;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Parties;

[TestFixture]
public class PartyTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    [Test]
    public async Task T074_Create_ShouldReturnPartyCode()
    {
        var result = await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "شركة اختبار", null, "TAX-001", null, null, null, null, null));

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);

        var party = await TestApp.FindAsync<Party>(result.Value);
        party.ShouldNotBeNull();
        party.PartyCode.ShouldStartWith("PTY-");
        party.PartyType.ShouldBe(PartyType.Supplier);
        party.NameAr.ShouldBe("شركة اختبار");
        party.TaxNumber.ShouldBe("TAX-001");
        party.IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task T074_GetById_ShouldReturnCorrectParty()
    {
        var createResult = await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Customer, "عميل", "Customer EN", null, null, "555-1234", null, null, null));
        createResult.Succeeded.ShouldBeTrue();

        var party = await TestApp.FindAsync<Party>(createResult.Value);
        party.ShouldNotBeNull();
        party.NameAr.ShouldBe("عميل");
        party.NameEn.ShouldBe("Customer EN");
        party.Phone.ShouldBe("555-1234");
    }

    [Test]
    public async Task T074_Update_ShouldModifyFields()
    {
        var createResult = await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Original Name", null, null, null, null, null, null, null));
        createResult.Succeeded.ShouldBeTrue();

        var updateResult = await TestApp.SendAsync(new UpdatePartyCommand(
            createResult.Value, PartyType.Supplier, "Updated Name", null, null, null, null, null, null, null));
        updateResult.Succeeded.ShouldBeTrue();

        var party = await TestApp.FindAsync<Party>(createResult.Value);
        party.ShouldNotBeNull();
        party.NameAr.ShouldBe("Updated Name");
    }

    [Test]
    public async Task T074_ToggleActive_ShouldFlipIsActive()
    {
        var createResult = await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Toggle Test", null, null, null, null, null, null, null));
        createResult.Succeeded.ShouldBeTrue();

        var party = await TestApp.FindAsync<Party>(createResult.Value);
        party.ShouldNotBeNull();
        party.IsActive.ShouldBeTrue();

        var toggleResult = await TestApp.SendAsync(new TogglePartyActiveCommand(createResult.Value));
        toggleResult.Succeeded.ShouldBeTrue();

        var toggled = await TestApp.FindAsync<Party>(createResult.Value);
        toggled.ShouldNotBeNull();
        toggled.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task T075_FilterByPartyType_ShouldReturnCorrectSubset()
    {
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "مورد 1", null, null, null, null, null, null, null));
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "مورد 2", null, null, null, null, null, null, null));
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Customer, "عميل 1", null, null, null, null, null, null, null));

        var suppliers = await TestApp.SendAsync(new GetPartiesQuery(PartyType: PartyType.Supplier));
        suppliers.Count.ShouldBe(2);
        suppliers.ShouldAllBe(p => p.PartyType == PartyType.Supplier);

        var customers = await TestApp.SendAsync(new GetPartiesQuery(PartyType: PartyType.Customer));
        customers.Count.ShouldBe(1);
        customers[0].PartyType.ShouldBe(PartyType.Customer);
    }

    [Test]
    public async Task T075_FilterByIsActive_ShouldReturnCorrectSubset()
    {
        var createResult = await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Active Party", null, null, null, null, null, null, null));
        createResult.Succeeded.ShouldBeTrue();

        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Another Active", null, null, null, null, null, null, null));

        await TestApp.SendAsync(new TogglePartyActiveCommand(createResult.Value));

        var active = await TestApp.SendAsync(new GetPartiesQuery(IsActive: true));
        active.ShouldAllBe(p => p.IsActive);

        var inactive = await TestApp.SendAsync(new GetPartiesQuery(IsActive: false));
        inactive.Count.ShouldBe(1);
        inactive[0].IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task T076_SearchByNameAr_ShouldMatch()
    {
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "شركة阿尔法 للتجارة", null, null, null, null, null, null, null));
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "شركة بيتا لل工程", null, null, null, null, null, null, null));

        var results = await TestApp.SendAsync(new GetPartiesQuery(Search: "阿尔法"));
        results.Count.ShouldBe(1);
        results[0].NameAr.ShouldContain("阿尔法");
    }

    [Test]
    public async Task T076_SearchByTaxNumber_ShouldMatch()
    {
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Company A", null, "TAX-100", null, null, null, null, null));
        await TestApp.SendAsync(new CreatePartyCommand(
            PartyType.Supplier, "Company B", null, "TAX-200", null, null, null, null, null));

        var results = await TestApp.SendAsync(new GetPartiesQuery(Search: "TAX-100"));
        results.Count.ShouldBe(1);
        results[0].TaxNumber.ShouldBe("TAX-100");
    }
}
