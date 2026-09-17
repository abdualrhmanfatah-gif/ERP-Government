using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Inventory.Entities;
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
        await SeedOpeningBalancesAsync();
        await SeedBudgetingDataAsync();
        await SeedBudgetClassificationsDataAsync();
        await SeedPaymentsDataAsync();
        await SeedPartiesDataAsync();
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
        else
        {
            var existing = await _context.DocumentSequences.Select(s => s.DocumentType).ToListAsync();
            var allSequences = DocumentSequenceSeedData.GetDocumentSequences();
            var missing = allSequences.Where(s => !existing.Contains(s.DocumentType)).ToList();
            if (missing.Count > 0)
            {
                _context.DocumentSequences.AddRange(missing);
                await _context.SaveChangesAsync();
            }
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

            var accountMapping = BudgetSeedData.GetAccountMapping();
            var accountsByCode = await _context.Accounts
                .Where(a => accountMapping.Values.Contains(a.Code))
                .ToDictionaryAsync(a => a.Code, a => a.Id, CancellationToken.None);

            foreach (var item in _context.BudgetItems)
            {
                if (accountMapping.TryGetValue(item.ItemCode, out var accountCode)
                    && accountsByCode.TryGetValue(accountCode, out var accountId))
                {
                    item.AccountId = accountId;
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

    private async Task SeedPartiesDataAsync()
    {
        if (!_context.Parties.Any())
        {
            var parties = PartySeedData.GetParties();
            _context.Parties.AddRange(parties);
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

        var desiredLocations = LocationSeedData.GetLocations();
        var existingLocationCodes = await _context.Locations
            .Select(l => l.Code)
            .ToListAsync();
        var existingLocationCodeSet = existingLocationCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingRootLocations = desiredLocations
            .Where(l => l.ParentLocationId is null && !existingLocationCodeSet.Contains(l.Code))
            .ToList();

        if (missingRootLocations.Count > 0)
        {
            _context.Locations.AddRange(missingRootLocations);
            await _context.SaveChangesAsync();
        }

        var locCodeToId = await _context.Locations
            .ToDictionaryAsync(l => l.Code, l => l.Id, StringComparer.OrdinalIgnoreCase);

        var missingChildLocations = new List<Location>();
        foreach (var loc in desiredLocations.Where(l => l.ParentLocationId.HasValue && !locCodeToId.ContainsKey(l.Code)))
        {
            if (!LocationSeedData.ParentCodeMap.TryGetValue(loc.Code, out var parentCode))
                throw new InvalidOperationException($"No parent code mapping for seed location '{loc.Code}'.");
            if (!locCodeToId.TryGetValue(parentCode, out var parentId))
                throw new InvalidOperationException($"Parent location '{parentCode}' was not found for seed location '{loc.Code}'.");

            loc.ParentLocationId = parentId;
            missingChildLocations.Add(loc);
        }

        if (missingChildLocations.Count > 0)
        {
            _context.Locations.AddRange(missingChildLocations);
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
        var desiredGroups = AssetGroupSeedData.GetAssetGroups();
        var existingCodes = await _context.AssetGroups
            .Select(g => g.Code)
            .ToListAsync();
        var existingCodeSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingRootGroups = desiredGroups
            .Where(g => g.ParentAssetGroupId is null && !existingCodeSet.Contains(g.Code))
            .Select(CloneAssetGroup)
            .ToList();

        if (missingRootGroups.Count > 0)
        {
            _context.AssetGroups.AddRange(missingRootGroups);
            await _context.SaveChangesAsync();
        }

        var codeToId = await _context.AssetGroups
            .ToDictionaryAsync(g => g.Code, g => g.Id, StringComparer.OrdinalIgnoreCase);

        var missingChildGroups = new List<AssetGroup>();
        foreach (var group in desiredGroups.Where(g => g.ParentAssetGroupId.HasValue && !codeToId.ContainsKey(g.Code)))
        {
            var parentCode = GetParentAssetGroupCode(group.Code);
            if (!codeToId.TryGetValue(parentCode, out var parentId))
                throw new InvalidOperationException($"Parent asset group '{parentCode}' was not found for seed group '{group.Code}'.");

            var childGroup = CloneAssetGroup(group);
            childGroup.ParentAssetGroupId = parentId;
            missingChildGroups.Add(childGroup);
        }

        if (missingChildGroups.Count > 0)
        {
            _context.AssetGroups.AddRange(missingChildGroups);
            await _context.SaveChangesAsync();
        }

        await MergeAssetGroupAccountsAsync();
        await SeedAssetAttributeDefinitionsAsync();
        await SeedAssetGroupAttributesAsync();
        await SeedAssetsFromSeedDataAsync();
        await SeedAssetAttributeValuesAsync();
    }

    private async Task MergeAssetGroupAccountsAsync()
    {
        var accountMappings = AssetGroupSeedData.AccountMappings;
        var codeToAccountId = await _context.Accounts
            .ToDictionaryAsync(a => a.Code, a => a.Id, StringComparer.OrdinalIgnoreCase);

        var groups = await _context.AssetGroups.ToListAsync();
        foreach (var group in groups)
        {
            if (!accountMappings.TryGetValue(group.Code, out var mapping))
                continue;

            bool changed = false;
            if (group.AssetAccountId is null && codeToAccountId.TryGetValue(mapping.AssetAccountCode, out var assetAccountId))
            {
                group.AssetAccountId = assetAccountId;
                changed = true;
            }
            if (group.AccumulatedDepreciationAccountId is null && codeToAccountId.TryGetValue(mapping.AccumulatedDepreciationAccountCode, out var depAccountId))
            {
                group.AccumulatedDepreciationAccountId = depAccountId;
                changed = true;
            }
            if (group.DepreciationExpenseAccountId is null && codeToAccountId.TryGetValue(mapping.DepreciationExpenseAccountCode, out var depExpenseAccountId))
            {
                group.DepreciationExpenseAccountId = depExpenseAccountId;
                changed = true;
            }
            if (group.DisposalAccountId is null && codeToAccountId.TryGetValue(mapping.DisposalAccountCode, out var disposalAccountId))
            {
                group.DisposalAccountId = disposalAccountId;
                changed = true;
            }
            if (changed)
                group.LastModified = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedAssetAttributeDefinitionsAsync()
    {
        var desired = AssetAttributeDefinitionSeedData.GetDefinitions();
        var existingCodes = await _context.AssetAttributeDefinitions
            .Select(d => d.Code)
            .ToListAsync();
        var existingCodeSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = desired.Where(d => !existingCodeSet.Contains(d.Code)).ToList();
        if (missing.Count > 0)
        {
            _context.AssetAttributeDefinitions.AddRange(missing);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssetGroupAttributesAsync()
    {
        var bindings = AssetGroupAttributeSeedData.Bindings;
        var codeToGroupId = await _context.AssetGroups
            .ToDictionaryAsync(g => g.Code, g => g.Id, StringComparer.OrdinalIgnoreCase);
        var codeToDefId = await _context.AssetAttributeDefinitions
            .ToDictionaryAsync(d => d.Code, d => d.Id, StringComparer.OrdinalIgnoreCase);

        var existingKeys = (await _context.AssetGroupAttributes
            .Select(a => new { a.AssetGroupId, a.AssetAttributeDefinitionId })
            .ToListAsync())
            .Select(a => (a.AssetGroupId, a.AssetAttributeDefinitionId))
            .ToHashSet();

        var missing = new List<AssetGroupAttribute>();
        foreach (var (groupCode, defCode, isRequired, sortOrder) in bindings)
        {
            if (!codeToGroupId.TryGetValue(groupCode, out var groupId)) continue;
            if (!codeToDefId.TryGetValue(defCode, out var defId)) continue;
            if (existingKeys.Contains((groupId, defId))) continue;

            missing.Add(new AssetGroupAttribute
            {
                AssetGroupId = groupId,
                AssetAttributeDefinitionId = defId,
                IsRequired = isRequired,
                SortOrder = sortOrder
            });
        }

        if (missing.Count > 0)
        {
            _context.AssetGroupAttributes.AddRange(missing);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssetsFromSeedDataAsync()
    {
        var blueprints = AssetSeedData.Blueprints;
        var existingCodes = await _context.Assets
            .Select(a => a.Code)
            .ToListAsync();
        var existingCodeSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var codeToGroupId = await _context.AssetGroups
            .ToDictionaryAsync(g => g.Code, g => g.Id, StringComparer.OrdinalIgnoreCase);
        var empToId = await _context.Employees
            .ToDictionaryAsync(e => e.EmployeeNumber, e => e.Id, StringComparer.OrdinalIgnoreCase);
        var locToId = await _context.Locations
            .ToDictionaryAsync(l => l.Code, l => l.Id, StringComparer.OrdinalIgnoreCase);
        var currToId = await _context.Currencies
            .ToDictionaryAsync(c => c.Code, c => c.Id, StringComparer.OrdinalIgnoreCase);

        var missing = new List<Asset>();
        foreach (var bp in blueprints)
        {
            if (existingCodeSet.Contains(bp.Code)) continue;
            if (!codeToGroupId.TryGetValue(bp.GroupCode, out var groupId)) continue;
            if (!currToId.TryGetValue(bp.CurrencyCode, out var currencyId)) continue;

            var asset = new Asset
            {
                Code = bp.Code,
                Name = bp.Name,
                Description = bp.Description,
                AssetGroupId = groupId,
                LocationId = bp.LocationCode is not null && locToId.TryGetValue(bp.LocationCode, out var locId) ? locId : null,
                EmployeeId = bp.EmployeeNumber is not null && empToId.TryGetValue(bp.EmployeeNumber, out var empId) ? empId : null,
                CurrencyId = currencyId,
                AssetTag = bp.AssetTag,
                Barcode = bp.Barcode,
                SerialNumber = bp.SerialNumber,
                OriginalValue = bp.OriginalValue,
                PurchaseDate = bp.PurchaseDate,
                ActivationDate = bp.ActivationDate,
                DepreciationStartDate = bp.DepreciationStartDate,
                Status = bp.Status,
                AcquisitionType = bp.AcquisitionType,
                UsefulLifeYears = bp.UsefulLifeYears,
                Notes = bp.Notes,
                IsActive = true
            };
            missing.Add(asset);
        }

        if (missing.Count > 0)
        {
            _context.Assets.AddRange(missing);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedAssetAttributeValuesAsync()
    {
        var blueprints = AssetAttributeValueSeedData.Blueprints;
        var codeToAssetId = await _context.Assets
            .ToDictionaryAsync(a => a.Code, a => a.Id, StringComparer.OrdinalIgnoreCase);
        var codeToDefId = await _context.AssetAttributeDefinitions
            .ToDictionaryAsync(d => d.Code, d => d.Id, StringComparer.OrdinalIgnoreCase);

        var existingKeys = (await _context.AssetAttributeValues
            .Select(v => new { v.AssetId, v.AssetAttributeDefinitionId })
            .ToListAsync())
            .Select(v => (v.AssetId, v.AssetAttributeDefinitionId))
            .ToHashSet();

        var missing = new List<AssetAttributeValue>();
        foreach (var bp in blueprints)
        {
            if (!codeToAssetId.TryGetValue(bp.AssetCode, out var assetId)) continue;
            if (!codeToDefId.TryGetValue(bp.DefinitionCode, out var defId)) continue;
            if (existingKeys.Contains((assetId, defId))) continue;

            missing.Add(new AssetAttributeValue
            {
                AssetId = assetId,
                AssetAttributeDefinitionId = defId,
                TextValue = bp.TextValue,
                IntegerValue = bp.IntegerValue,
                DateValue = bp.DateValue,
                BooleanValue = bp.BooleanValue
            });
        }

        if (missing.Count > 0)
        {
            _context.AssetAttributeValues.AddRange(missing);
            await _context.SaveChangesAsync();
        }
    }

    private static AssetGroup CloneAssetGroup(AssetGroup group) => new()
    {
        Code = group.Code,
        Name = group.Name,
        Description = group.Description,
        ParentAssetGroupId = group.ParentAssetGroupId,
        AssetAccountId = group.AssetAccountId,
        AccumulatedDepreciationAccountId = group.AccumulatedDepreciationAccountId,
        DepreciationExpenseAccountId = group.DepreciationExpenseAccountId,
        DisposalAccountId = group.DisposalAccountId,
        DepreciationMethod = group.DepreciationMethod,
        DepreciationRate = group.DepreciationRate,
        DefaultUsefulLifeYears = group.DefaultUsefulLifeYears,
        ResidualValuePercentage = group.ResidualValuePercentage,
        IsDepreciable = group.IsDepreciable,
        AssetCategory = group.AssetCategory,
        IsActive = group.IsActive
    };

    private static string GetParentAssetGroupCode(string code)
    {
        var lastDashIndex = code.LastIndexOf('-');
        if (lastDashIndex <= 0)
            throw new InvalidOperationException($"Seed asset group code '{code}' does not include a parent code.");

        return code[..lastDashIndex];
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

        if (!_context.JournalEntryTemplates.Any())
        {
            var templates = JournalEntryTemplateSeedData.GetTemplates();
            _context.JournalEntryTemplates.AddRange(templates);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedOpeningBalancesAsync()
    {
        if (_context.JournalEntries.Any(e => e.EntryType == MoveEntryType.Opening))
            return;

        foreach (var bp in JournalEntrySeedData.All)
        {
            var fiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.YearNumber == bp.FiscalYearNumber);
            if (fiscalYear is null) continue;

            var period = await _context.FiscalPeriods
                .FirstOrDefaultAsync(p => p.FiscalYearId == fiscalYear.Id
                                       && bp.DocumentDate >= p.StartDate
                                       && bp.DocumentDate <= p.EndDate);
            if (period is null) continue;

            var codeToAccountId = await _context.Accounts
                .ToDictionaryAsync(a => a.Code, a => a.Id);

            var baseCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsBase && c.IsActive);
            var currencyId = baseCurrency?.Id ?? 1;

            var entry = new JournalEntry
            {
                EntryNumber = $"OB-{bp.FiscalYearNumber}",
                DocumentDate = bp.DocumentDate,
                PostingDate = bp.DocumentDate,
                EntryType = MoveEntryType.Opening,
                EntryStatus = EntryStatus.Posted,
                PeriodId = period.Id,
                FiscalYearId = fiscalYear.Id,
                Narration = bp.Narration,
                IsSystemGenerated = true,
                PostedAt = DateTimeOffset.UtcNow
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            var sequence = 0;
            foreach (var line in bp.Lines)
            {
                if (!codeToAccountId.TryGetValue(line.AccountCode, out var accountId))
                    continue;

                _context.JournalEntryLines.Add(new JournalEntryLine
                {
                    JournalEntryId = entry.Id,
                    Sequence = ++sequence,
                    AccountId = accountId,
                    Description = line.Description,
                    CurrencyId = currencyId,
                    ExchangeRate = 1,
                    Debit = line.Debit,
                    Credit = line.Credit
                });
            }

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
