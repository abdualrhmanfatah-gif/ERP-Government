using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Constants;
using ERP_Government.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default security roles
        await SeedSecurityDataAsync();
        await SeedFinancialSettingsDataAsync();
        await SeedOrganizationDataAsync();
        await SeedAccountingDataAsync();
        await SeedJournalsDataAsync();
        await SeedBudgetingDataAsync();
        await SeedBudgetClassificationsDataAsync();
        await SeedPaymentsDataAsync();
        await SeedInventoryDataAsync();
        await SeedProcurementDataAsync();
        await SeedCommitteesDataAsync();
        await SeedAssetsDataAsync();

        // Seed default admin user directly (no Identity)
        if (!_context.Users.Any())
        {
            var adminRoleId = _context.SecurityRoles.First(r => r.Code == "ADMIN").Id;
            var hasher = new PasswordHasher<ERP_Government.Domain.Security.Entities.User>();
            var adminUser = new ERP_Government.Domain.Security.Entities.User
            {
                Login = "administrator@localhost",
                IsActive = true,
                AccountType = ERP_Government.Domain.Security.Enums.AccountType.Internal,
                RoleId = adminRoleId
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Administrator1!");
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAccountingDataAsync()
    {
        if (!_context.AccountGroups.Any())
        {
            var roots = AccountGroupSeedData.GetRootAccountGroups();
            _context.AccountGroups.AddRange(roots);
            await _context.SaveChangesAsync();

            var codeToId = _context.AccountGroups.ToDictionary(g => g.Code, g => g.Id);

            var childGroups = AccountGroupSeedData.GetChildAccountGroups();
            foreach (var child in childGroups)
            {
                if (child.ParentCode != null && codeToId.TryGetValue(child.ParentCode, out var parentId))
                    child.ParentId = parentId;
            }
            _context.AccountGroups.AddRange(childGroups);
            await _context.SaveChangesAsync();
        }

        if (!_context.Accounts.Any())
        {
            var accounts = AccountSeedData.GetAccounts();
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedSecurityDataAsync()
    {
        if (!_context.SecurityRoles.Any())
        {
            var roles = SecurityRoleSeedData.GetRoles();
            _context.SecurityRoles.AddRange(roles);
            await _context.SaveChangesAsync();
        }

        // Replace old MODULE_ACTION permissions with PermissionCodes-based ones
        // (old codes like COMMITTEES_CREATE never match queries using Committees.Create)
        if (!_context.SecurityPermissions.Any() || !_context.RolePermissions.Any())
        {
            // Clear old permissions + role-permissions (old format useless)
            _context.RolePermissions.RemoveRange(_context.RolePermissions);
            _context.SecurityPermissions.RemoveRange(_context.SecurityPermissions);
            await _context.SaveChangesAsync();

            // Seed PermissionCodes-based permissions
            var allPerms = RolePermissionSeedData.GetAllPermissionCodesStatic();
            _context.SecurityPermissions.AddRange(allPerms);
            await _context.SaveChangesAsync();

            // Link ALL to ADMIN
            var roles = _context.SecurityRoles.ToList();
            var permissions = _context.SecurityPermissions.ToList();
            var (_, rolePerms) = RolePermissionSeedData.Seed(roles, permissions);
            _context.RolePermissions.AddRange(rolePerms);
            await _context.SaveChangesAsync();
        }

        if (!_context.ApprovalRules.Any())
        {
            var approvalRules = ApprovalRuleSeedData.GetApprovalRules();
            _context.ApprovalRules.AddRange(approvalRules);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedFinancialSettingsDataAsync()
    {
        if (!_context.Currencies.Any())
        {
            var currencies = CurrencySeedData.GetCurrencies();
            _context.Currencies.AddRange(currencies);
            await _context.SaveChangesAsync();
        }

        if (!_context.FiscalYears.Any())
        {
            var fiscalYears = FiscalYearSeedData.GetFiscalYears();
            _context.FiscalYears.AddRange(fiscalYears);
            await _context.SaveChangesAsync();
        }

        if (!_context.FiscalPeriods.Any())
        {
            var fiscalPeriods = FiscalPeriodSeedData.GetFiscalPeriods();
            _context.FiscalPeriods.AddRange(fiscalPeriods);
            await _context.SaveChangesAsync();
        }

        if (!_context.ExchangeRates.Any())
        {
            var exchangeRates = ExchangeRateSeedData.GetExchangeRates();
            _context.ExchangeRates.AddRange(exchangeRates);
            await _context.SaveChangesAsync();
        }

        if (!_context.DocumentSequences.Any())
        {
            var documentSequences = DocumentSequenceSeedData.GetDocumentSequences();
            _context.DocumentSequences.AddRange(documentSequences);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedOrganizationDataAsync()
    {
        if (!_context.OrganizationalUnits.Any())
        {
            var units = OrganizationalUnitSeedData.GetUnits();
            _context.OrganizationalUnits.AddRange(units);
            await _context.SaveChangesAsync();
        }

        if (!_context.CostCenters.Any())
        {
            var costCenters = CostCenterSeedData.GetCostCenters();
            _context.CostCenters.AddRange(costCenters);
            await _context.SaveChangesAsync();
        }

        if (!_context.Employees.Any())
        {
            var employees = EmployeeSeedData.GetEmployees();
            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();
        }

        if (!_context.Projects.Any())
        {
            var projects = ProjectSeedData.GetProjects();
            _context.Projects.AddRange(projects);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedBudgetingDataAsync()
    {
        // TODO: Recreate seed data in Phase 2 (T016-T017)
        await Task.CompletedTask;
    }

    private async Task SeedPaymentsDataAsync()
    {
        if (!_context.BankAccounts.Any())
        {
            var bankAccounts = BankAccountSeedData.GetBankAccounts();
            _context.BankAccounts.AddRange(bankAccounts);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedInventoryDataAsync()
    {
        if (!_context.Units.Any())
        {
            var units = UnitSeedData.GetUnits();
            _context.Units.AddRange(units);
            await _context.SaveChangesAsync();
        }

        if (!_context.ItemCategories.Any())
        {
            var itemCategories = ItemCategorySeedData.GetItemCategories();
            _context.ItemCategories.AddRange(itemCategories);
            await _context.SaveChangesAsync();
        }

        if (!_context.Warehouses.Any())
        {
            var warehouses = WarehouseSeedData.GetWarehouses();
            _context.Warehouses.AddRange(warehouses);
            await _context.SaveChangesAsync();
        }

        if (!_context.Locations.Any())
        {
            var locations = LocationSeedData.GetLocations();
            _context.Locations.AddRange(locations);
            await _context.SaveChangesAsync();
        }

        if (!_context.Items.Any())
        {
            var items = ItemSeedData.GetItems();
            _context.Items.AddRange(items);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedProcurementDataAsync()
    {
    }

    private async Task SeedCommitteesDataAsync()
    {
        if (!_context.Committees.Any())
        {
            var committees = CommitteeSeedData.GetCommittees();
            _context.Committees.AddRange(committees);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssetsDataAsync()
    {
        if (!_context.AssetGroups.Any())
        {
            var assetGroups = AssetGroupSeedData.GetAssetGroups();
            _context.AssetGroups.AddRange(assetGroups);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedJournalsDataAsync()
    {
        if (!_context.Journals.Any())
        {
            var journals = JournalSeedData.GetJournals();
            _context.Journals.AddRange(journals);
            await _context.SaveChangesAsync();
        }

        if (!_context.PostingRules.Any())
        {
            var postingRules = PostingRuleSeedData.GetPostingRules();
            _context.PostingRules.AddRange(postingRules);
            await _context.SaveChangesAsync();
        }

        if (!_context.JournalEntryTemplates.Any())
        {
            var templates = JournalEntryTemplateSeedData.GetTemplates();
            _context.JournalEntryTemplates.AddRange(templates);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedBudgetClassificationsDataAsync()
    {
        // TODO: Recreate seed data in Phase 2 (T017)
        await Task.CompletedTask;
    }
}
