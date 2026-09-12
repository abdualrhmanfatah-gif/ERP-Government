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
        await MergeDocumentAttachmentRequirementsAsync();
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
        await SeedUsersDataAsync();
    }

    private async Task SeedAccountingDataAsync()
    {
        await AccountingChartSeeder.SeedAsync(_context, CancellationToken.None);
    }

    private async Task SeedSecurityDataAsync()
    {
        if (!_context.SecurityRoles.Any())
        {
            var roles = SecurityRoleSeedData.GetRoles();
            _context.SecurityRoles.AddRange(roles);
            await _context.SaveChangesAsync();
        }
        else
        {
            await MergeSecurityRolesAsync();
        }

        // Merge PermissionCodes-based permissions without deleting existing custom data.
        await MergeSecurityPermissionsAsync();

        if (!_context.ApprovalRules.Any())
        {
            var approvalRules = ApprovalRuleSeedData.GetApprovalRules();
            _context.ApprovalRules.AddRange(approvalRules);
            await _context.SaveChangesAsync();
        }
    }

    private async Task MergeSecurityPermissionsAsync()
    {
        var desiredPermissions = RolePermissionSeedData.GetAllPermissionCodesStatic();
        var existingPermissions = await _context.SecurityPermissions
            .ToDictionaryAsync(permission => permission.Code);

        var missingPermissions = desiredPermissions
            .Where(permission => !existingPermissions.ContainsKey(permission.Code))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            _context.SecurityPermissions.AddRange(missingPermissions);
            await _context.SaveChangesAsync();
        }

        var roles = await _context.SecurityRoles.ToListAsync();
        var permissions = await _context.SecurityPermissions.ToListAsync();
        var (_, desiredRolePermissions) = RolePermissionSeedData.Seed(roles, permissions);

        var existingRolePermissionKeys = (await _context.RolePermissions
            .Select(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .ToListAsync())
            .Select(rolePermission => (rolePermission.RoleId, rolePermission.PermissionId))
            .ToHashSet();

        var missingRolePermissions = desiredRolePermissions
            .Where(rolePermission => !existingRolePermissionKeys.Contains((rolePermission.RoleId, rolePermission.PermissionId)))
            .Select(rolePermission => new Domain.Security.Entities.RolePermission
            {
                RoleId = rolePermission.RoleId,
                PermissionId = rolePermission.PermissionId
            })
            .ToList();

        if (missingRolePermissions.Count > 0)
        {
            _context.RolePermissions.AddRange(missingRolePermissions);
            await _context.SaveChangesAsync();
        }
    }

    private async Task MergeSecurityRolesAsync()
    {
        var desiredRoles = SecurityRoleSeedData.GetRoles();
        var existingRoleCodes = (await _context.SecurityRoles.Select(r => r.Code).ToListAsync()).ToHashSet();

        var missingRoles = desiredRoles
            .Where(r => !existingRoleCodes.Contains(r.Code))
            .ToList();

        if (missingRoles.Count > 0)
        {
            _context.SecurityRoles.AddRange(missingRoles);
            await _context.SaveChangesAsync();
        }
    }

    private async Task MergeDocumentAttachmentRequirementsAsync()
    {
        var desired = DisbursementRequestAttachmentSeedData.GetRequirements();
        var existing = await _context.DocumentAttachmentRequirements
            .ToDictionaryAsync(r => $"{r.DocumentType}:{r.AttachmentTypeCode}");

        var missing = desired
            .Where(d => !existing.ContainsKey($"{d.DocumentType}:{d.AttachmentTypeCode}"))
            .ToList();

        if (missing.Count > 0)
        {
            _context.DocumentAttachmentRequirements.AddRange(missing);
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
        if (!_context.Funds.Any())
        {
            var funds = FundSeedData.GetFunds();
            _context.Funds.AddRange(funds);
            await _context.SaveChangesAsync();
        }

        if (!_context.BudgetTypes.Any())
        {
            var budgetTypes = BudgetTypeSeedData.GetBudgetTypes();
            _context.BudgetTypes.AddRange(budgetTypes);
            await _context.SaveChangesAsync();
        }

        if (!_context.Budgets.Any())
        {
            var budgets = BudgetSeedData.GetBudgets();

            var fundId = _context.Funds.OrderBy(f => f.Id).First().Id;
            var budgetTypeId = _context.BudgetTypes.OrderBy(bt => bt.Id).First().Id;
            var fiscalYearId = _context.FiscalYears.OrderBy(f => f.Id).First(f => f.YearNumber == 2026).Id;

            foreach (var budget in budgets)
            {
                budget.FundId = fundId;
                budget.BudgetTypeId = budgetTypeId;
                budget.FiscalYearId = fiscalYearId;
            }

            _context.Budgets.AddRange(budgets);
            await _context.SaveChangesAsync();

            // Seed BudgetItems with hierarchy
            var items = BudgetSeedData.GetBudgetItems();
            var savedBudgetIds = _context.Budgets.OrderBy(b => b.Id).Select(b => b.Id).ToList();
            var budgetId = savedBudgetIds[0];
            var secondBudgetId = savedBudgetIds[1];

            // Budget 1 items (1000-3xxx)
            var budget1Items = items.Where(i => !i.ItemCode.StartsWith("INV-")).ToList();
            foreach (var item in budget1Items)
            {
                item.BudgetId = budgetId;
            }

            // Budget 2 items (INV-xxx)
            var budget2Items = items.Where(i => i.ItemCode.StartsWith("INV-")).ToList();
            foreach (var item in budget2Items)
            {
                item.BudgetId = secondBudgetId;
            }

            _context.BudgetItems.AddRange(budget1Items);
            _context.BudgetItems.AddRange(budget2Items);
            await _context.SaveChangesAsync();

            // Set ParentId hierarchy by ItemCode lookup
            var codeToId = _context.BudgetItems.ToDictionary(i => i.ItemCode, i => i.Id);

            var parentMap = new Dictionary<string, string>
            {
                // Budget 1 level 2 → level 1
                ["1100"] = "1000", ["1200"] = "1000",
                ["2100"] = "2000", ["2200"] = "2000", ["2300"] = "2000",
                ["3100"] = "3000", ["3200"] = "3000", ["3300"] = "3000",
                // Budget 1 level 3 → level 2
                ["1110"] = "1100", ["1120"] = "1100",
                ["3110"] = "3100", ["3120"] = "3100",
                // Budget 2 level 2 → level 1
                ["INV-1100"] = "INV-1000", ["INV-1200"] = "INV-1000",
                ["INV-2100"] = "INV-2000",
                ["INV-3100"] = "INV-3000", ["INV-3200"] = "INV-3000",
            };

            foreach (var item in _context.BudgetItems)
            {
                if (parentMap.TryGetValue(item.ItemCode, out var parentCode) && codeToId.TryGetValue(parentCode, out var parentId))
                {
                    item.ParentId = parentId;
                }
            }

            await _context.SaveChangesAsync();
        }
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

    private async Task SeedUsersDataAsync()
    {
        // Seed admin user
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
        else
        {
            // Ensure admin user has ADMIN role
            var adminRoleId = _context.SecurityRoles.First(r => r.Code == "ADMIN").Id;
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == "administrator@localhost");
            if (adminUser is not null && adminUser.RoleId != adminRoleId)
            {
                adminUser.RoleId = adminRoleId;
                await _context.SaveChangesAsync();
            }
        }

        // Seed example users
        var existingLogins = await _context.Users.Select(u => u.Login).ToListAsync();
        var roleCodeToId = await _context.SecurityRoles.ToDictionaryAsync(r => r.Code, r => r.Id);

        var seedUsers = UserSeedData.GetUsers();
        foreach (var (user, roleCode) in seedUsers)
        {
            if (!existingLogins.Contains(user.Login) && roleCodeToId.TryGetValue(roleCode, out var roleId))
            {
                user.RoleId = roleId;
                _context.Users.Add(user);
            }
        }
        await _context.SaveChangesAsync();
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

        if (!_context.PostingRuleLines.Any())
        {
            var paymentOrderRule = await _context.PostingRules
                .FirstOrDefaultAsync(r => r.EventType == "PaymentOrderExecuted");
            if (paymentOrderRule is not null)
            {
                var lines = PostingRuleLineSeedData.GetLinesForPaymentOrderExecuted(paymentOrderRule.Id);
                _context.PostingRuleLines.AddRange(lines);
                await _context.SaveChangesAsync();
            }
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
        if (!_context.BudgetClassifications.Any())
        {
            var classifications = BudgetClassificationSeedData.GetClassifications();
            _context.BudgetClassifications.AddRange(classifications);
            await _context.SaveChangesAsync();

            // Set ParentId hierarchy by Code lookup
            var codeToId = _context.BudgetClassifications.ToDictionary(c => c.Code, c => c.Id);
            var parentMap = new Dictionary<string, string>
            {
                ["2100"] = "2000",
                ["2200"] = "2000",
                ["2110"] = "2100",
            };

            foreach (var c in _context.BudgetClassifications)
            {
                if (parentMap.TryGetValue(c.Code, out var parentCode) && codeToId.TryGetValue(parentCode, out var parentId))
                {
                    c.ParentId = parentId;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
