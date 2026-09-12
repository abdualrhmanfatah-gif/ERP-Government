using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Seeds RolePermissions matching PermissionCodes constants used by [Authorize(Policy=...)] attributes.
/// ADMIN gets ALL permissions. Other roles get domain-specific sets.
/// </summary>
public static class RolePermissionSeedData
{
    public static List<SecurityPermission> GetAllPermissionCodesStatic() => GetAllPermissionCodes();

    public static (List<SecurityPermission> perms, List<RolePermission> rolePerms) Seed(
        List<SecurityRole> roles, List<SecurityPermission> allPerms)
    {
        var rolePerms = new List<RolePermission>();

        // ADMIN gets ALL
        var adminRole = roles.First(r => r.Code == "ADMIN");
        foreach (var p in allPerms)
            rolePerms.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });

        // FIN_MGR — محاسبة + موازنة + مدفوعات
        var finMgr = roles.First(r => r.Code == "FIN_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Budgets.") || p.Code.StartsWith("BudgetItems.") ||
            p.Code.StartsWith("BudgetItemAllocations.") ||
            p.Code.StartsWith("BudgetClassifications.") || p.Code.StartsWith("BudgetTypes.") ||
            p.Code.StartsWith("BudgetTransactions.") ||
            p.Code.StartsWith("Encumbrances.") ||
            p.Code.StartsWith("Funds.") ||
            p.Code.StartsWith("BankAccounts.") || p.Code.StartsWith("DisbursementRequests.") ||
            p.Code.StartsWith("PaymentOrders.") || p.Code.StartsWith("Payments.") ||
            p.Code.StartsWith("Accounting.") || p.Code.StartsWith("FiscalYears.") ||
            p.Code.StartsWith("FiscalPeriods.") || p.Code.StartsWith("Currencies.") ||
            p.Code.StartsWith("ExchangeRates.") || p.Code.StartsWith("ClosingEntries.") ||
            p.Code.StartsWith("DocumentSequences.") ||
            p.Code.StartsWith("Reports.") || p.Code.StartsWith("Reporting.")))
            rolePerms.Add(new RolePermission { RoleId = finMgr.Id, PermissionId = p.Id });

        // ACCT_SR — محاسب أول: مراجعة + اعتماد + ترحيل
        var acctSr = roles.First(r => r.Code == "ACCT_SR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Accounting.") || p.Code.StartsWith("FiscalYears.") ||
            p.Code.StartsWith("FiscalPeriods.") || p.Code.StartsWith("ClosingEntries.") ||
            p.Code.StartsWith("Reports.") || p.Code.StartsWith("Reporting.") || p.Code.StartsWith("Currencies.") ||
            p.Code.StartsWith("ExchangeRates.")))
            rolePerms.Add(new RolePermission { RoleId = acctSr.Id, PermissionId = p.Id });

        // ACCT — محاسب: إنشاء + تعديل القيود
        var acct = roles.First(r => r.Code == "ACCT");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Accounting.Journals.") || p.Code.StartsWith("Accounting.JournalEntries.") ||
            p.Code.StartsWith("Accounting.ChartOfAccounts.") || p.Code.StartsWith("Accounting.Templates.") ||
            p.Code.StartsWith("Accounting.RecurringEntries.") || p.Code.StartsWith("Reports.") || p.Code.StartsWith("Reporting.")))
            rolePerms.Add(new RolePermission { RoleId = acct.Id, PermissionId = p.Id });

        // AUDITOR — مراجع: قراءة + تقارير فقط
        var auditor = roles.First(r => r.Code == "AUDITOR");
        foreach (var p in allPerms.Where(p =>
            p.Code.EndsWith(".View") || p.Code.EndsWith(".Read") ||
            p.Code.StartsWith("Reports.") || p.Code.StartsWith("Reporting.") || p.Code.StartsWith("Accounting.Reports.")))
            rolePerms.Add(new RolePermission { RoleId = auditor.Id, PermissionId = p.Id });

        // BUD_MGR — مدير الموازنة: موازنة شاملة
        var budMgr = roles.First(r => r.Code == "BUD_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Budgets.") || p.Code.StartsWith("BudgetItems.") ||
            p.Code.StartsWith("BudgetItemAllocations.") ||
            p.Code.StartsWith("BudgetClassifications.") || p.Code.StartsWith("BudgetTypes.") ||
            p.Code.StartsWith("BudgetTransactions.") ||
            p.Code.StartsWith("Encumbrances.") ||
            p.Code.StartsWith("Funds.") || p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = budMgr.Id, PermissionId = p.Id });

        // BUD_OFF — موظف موازنة: إدخال بيانات
        var budOff = roles.First(r => r.Code == "BUD_OFF");
        foreach (var p in allPerms.Where(p =>
            (p.Code.StartsWith("Budgets.") && (p.Code.EndsWith(".View") || p.Code.EndsWith(".Create") || p.Code.EndsWith(".Submit"))) ||
            (p.Code.StartsWith("BudgetItems.") && !p.Code.EndsWith(".Delete")) ||
            (p.Code.StartsWith("BudgetItemAllocations.") && !p.Code.EndsWith(".Delete")) ||
            (p.Code.StartsWith("BudgetClassifications.") && p.Code.EndsWith(".View")) ||
            (p.Code.StartsWith("BudgetTypes.") && p.Code.EndsWith(".View")) ||
            p.Code.StartsWith("Funds.") && p.Code.EndsWith(".View")))
            rolePerms.Add(new RolePermission { RoleId = budOff.Id, PermissionId = p.Id });

        // PAY_MGR — مدير المدفوعات: أوامر دفع + تسجيل مدفوعات
        var payMgr = roles.First(r => r.Code == "PAY_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("PaymentOrders.") || p.Code.StartsWith("Payments.") ||
            p.Code.StartsWith("BankAccounts.") ||
            p.Code.StartsWith("Currencies.") || p.Code.StartsWith("FiscalYears.") || p.Code.StartsWith("FiscalPeriods.") ||
            p.Code.StartsWith("DisbursementRequests.") ||
            p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = payMgr.Id, PermissionId = p.Id });

        // PAY_OFF — موظف مدفوعات: إنشاء أوامر دفع
        var payOff = roles.First(r => r.Code == "PAY_OFF");
        foreach (var p in allPerms.Where(p =>
            (p.Code.StartsWith("PaymentOrders.") && (p.Code.EndsWith(".View") || p.Code.EndsWith(".Create") || p.Code.EndsWith(".Submit"))) ||
            (p.Code.StartsWith("DisbursementRequests.") && (p.Code.EndsWith(".View") || p.Code.EndsWith(".Create") || p.Code.EndsWith(".Submit"))) ||
            (p.Code.StartsWith("BankAccounts.") && p.Code.EndsWith(".View"))))
            rolePerms.Add(new RolePermission { RoleId = payOff.Id, PermissionId = p.Id });

        // AccountsManager — مدير الحسابات: توقيع أول على طلبات الصرف + موازنة شاملة + محاسبة + مدفوعات
        var acctMgr = roles.First(r => r.Code == "AccountsManager");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("DisbursementRequests.") || p.Code.StartsWith("PaymentOrders.") ||
            p.Code.StartsWith("Payments.") || p.Code.StartsWith("BankAccounts.") ||
            p.Code.StartsWith("Reports.") ||
            p.Code.StartsWith("Budgets.") || p.Code.StartsWith("BudgetItems.") ||
            p.Code.StartsWith("BudgetItemAllocations.") ||
            p.Code.StartsWith("BudgetClassifications.") || p.Code.StartsWith("BudgetTypes.") ||
            p.Code.StartsWith("BudgetTransactions.") ||
            p.Code.StartsWith("Encumbrances.") ||
            p.Code.StartsWith("Funds.") ||
            p.Code.StartsWith("Accounting.") || p.Code.StartsWith("FiscalYears.") ||
            p.Code.StartsWith("FiscalPeriods.") || p.Code.StartsWith("Currencies.") ||
            p.Code.StartsWith("ExchangeRates.") || p.Code.StartsWith("ClosingEntries.") ||
            p.Code.StartsWith("DocumentSequences.") ||
            p.Code.StartsWith("Reporting.")))
            rolePerms.Add(new RolePermission { RoleId = acctMgr.Id, PermissionId = p.Id });

        // AuthorizingOfficer — جهاز الأمر: توقيع ثاني + إنشاء أمر الصرف
        var authOff = roles.First(r => r.Code == "AuthorizingOfficer");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("DisbursementRequests.") || p.Code.StartsWith("PaymentOrders.") ||
            p.Code.StartsWith("BankAccounts.") || p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = authOff.Id, PermissionId = p.Id });

        // PROC_MGR — مدير المشتريات
        var procMgr = roles.First(r => r.Code == "PROC_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Parties.") || p.Code.StartsWith("PurchaseRequests.") ||
            p.Code.StartsWith("RFQ.") || p.Code.StartsWith("Quotations.") ||
            p.Code.StartsWith("PurchaseOrders.") || p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = procMgr.Id, PermissionId = p.Id });

        // PROC_OFF — موظف مشتريات
        var procOff = roles.First(r => r.Code == "PROC_OFF");
        foreach (var p in allPerms.Where(p =>
            (p.Code.StartsWith("Parties.") && p.Code.EndsWith(".View")) ||
            (p.Code.StartsWith("PurchaseRequests.") && (p.Code.EndsWith(".View") || p.Code.EndsWith(".Create") || p.Code.EndsWith(".Submit"))) ||
            (p.Code.StartsWith("RFQ.") && (p.Code.EndsWith(".View") || p.Code.EndsWith(".Create"))) ||
            (p.Code.StartsWith("Quotations.") && p.Code.EndsWith(".View")) ||
            (p.Code.StartsWith("PurchaseOrders.") && p.Code.EndsWith(".View"))))
            rolePerms.Add(new RolePermission { RoleId = procOff.Id, PermissionId = p.Id });

        // HR_MGR — مدير الموارد البشرية
        var hrMgr = roles.First(r => r.Code == "HR_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Employees.") || p.Code.StartsWith("OrgUnits.") ||
            p.Code.StartsWith("CostCenters.") || p.Code.StartsWith("Projects.") ||
            p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = hrMgr.Id, PermissionId = p.Id });

        // ASST_MGR — مدير الأصول
        var asstMgr = roles.First(r => r.Code == "ASST_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Assets.") || p.Code.StartsWith("AssetGroups.") ||
            p.Code.StartsWith("AssetMovements.") || p.Code.StartsWith("AssetDisposals.") ||
            p.Code.StartsWith("AssetRevaluations.") || p.Code.StartsWith("AssetImpairments.") ||
            p.Code.StartsWith("Depreciation.") || p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = asstMgr.Id, PermissionId = p.Id });

        // INV_MGR — مدير المخزون
        var invMgr = roles.First(r => r.Code == "INV_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("Items.") || p.Code.StartsWith("ItemCategories.") ||
            p.Code.StartsWith("Units.") || p.Code.StartsWith("Warehouses.") ||
            p.Code.StartsWith("Locations.") || p.Code.StartsWith("GoodsReceiptNotes.") ||
            p.Code.StartsWith("StockTakes.") || p.Code.StartsWith("StockTransactions.") ||
            p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = invMgr.Id, PermissionId = p.Id });

        // REV_MGR — مدير الإيرادات
        var revMgr = roles.First(r => r.Code == "REV_MGR");
        foreach (var p in allPerms.Where(p =>
            p.Code.StartsWith("RevenueReceipts.") || p.Code.StartsWith("Reports.")))
            rolePerms.Add(new RolePermission { RoleId = revMgr.Id, PermissionId = p.Id });

        // VIEWER — مستعرض: قراءة فقط
        var viewer = roles.First(r => r.Code == "VIEWER");
        foreach (var p in allPerms.Where(p =>
            p.Code.EndsWith(".View") || p.Code.EndsWith(".Read") ||
            p.Code.StartsWith("Reports.") || p.Code.StartsWith("Accounting.Reports.")))
            rolePerms.Add(new RolePermission { RoleId = viewer.Id, PermissionId = p.Id });

        return (allPerms, rolePerms);
    }

    private static List<SecurityPermission> GetAllPermissionCodes()
    {
        return
        [
            // Financial Settings
            Make("Currencies.View"), Make("Currencies.Create"), Make("Currencies.Update"), Make("Currencies.Activate"), Make("Currencies.Deactivate"),
            Make("ExchangeRates.View"), Make("ExchangeRates.Create"), Make("ExchangeRates.Update"), Make("ExchangeRates.Activate"), Make("ExchangeRates.Deactivate"),
            Make("FiscalYears.View"), Make("FiscalYears.Create"), Make("FiscalYears.Update"), Make("FiscalYears.Open"), Make("FiscalYears.Close"),
            Make("FiscalPeriods.View"), Make("FiscalPeriods.Create"), Make("FiscalPeriods.Update"), Make("FiscalPeriods.Lock"), Make("FiscalPeriods.Unlock"),
            Make("ClosingEntries.View"), Make("ClosingEntries.Generate"), Make("ClosingEntries.Approve"), Make("ClosingEntries.Reverse"),
            Make("DocumentSequences.View"), Make("DocumentSequences.Create"), Make("DocumentSequences.Update"), Make("DocumentSequences.Deactivate"),

            // Accounting
            Make("Accounting.ChartOfAccounts.Read"), Make("Accounting.ChartOfAccounts.Create"), Make("Accounting.ChartOfAccounts.Edit"),
            Make("Accounting.Journals.Read"), Make("Accounting.Journals.Create"), Make("Accounting.Journals.Edit"),
            Make("Accounting.JournalEntries.Read"), Make("Accounting.JournalEntries.Create"), Make("Accounting.JournalEntries.Submit"),
            Make("Accounting.JournalEntries.Approve"), Make("Accounting.JournalEntries.Post"), Make("Accounting.JournalEntries.Reverse"),
            Make("Accounting.JournalEntries.UpdateLines"), Make("Accounting.JournalEntries.Cancel"),
            Make("Accounting.PostingRules.Read"), Make("Accounting.PostingRules.Create"), Make("Accounting.PostingRules.Edit"), Make("Accounting.PostingRules.Delete"),
            Make("Accounting.Templates.Read"), Make("Accounting.Templates.Create"), Make("Accounting.Templates.Update"),
            Make("Accounting.RecurringEntries.Read"), Make("Accounting.RecurringEntries.Create"), Make("Accounting.RecurringEntries.Pause"),
            Make("Accounting.RecurringEntries.Resume"), Make("Accounting.RecurringEntries.Cancel"),
            Make("Accounting.Reports.BalanceSheet"), Make("Accounting.Reports.IncomeStatement"), Make("Accounting.Reports.GeneralLedger"),
            Make("Accounting.Reports.CashFlow"), Make("Accounting.Reports.TrialBalance"), Make("Accounting.Reports.Export"), Make("Accounting.Reports.Print"),

            // Budgeting
            Make("Funds.View"), Make("Funds.Create"), Make("Funds.Update"), Make("Funds.ToggleActive"),
            Make("Budgets.View"), Make("Budgets.Create"), Make("Budgets.Update"), Make("Budgets.Submit"), Make("Budgets.Approve"), Make("Budgets.Activate"),
            Make("Budgets.Suspend"), Make("Budgets.Close"), Make("Budgets.Cancel"),
            Make("BudgetItems.View"), Make("BudgetItems.Create"), Make("BudgetItems.Update"), Make("BudgetItems.Delete"), Make("BudgetItems.Move"),
            Make("BudgetClassifications.View"), Make("BudgetClassifications.Create"), Make("BudgetClassifications.Update"), Make("BudgetClassifications.ToggleActive"),
            Make("BudgetTypes.View"), Make("BudgetTypes.Create"), Make("BudgetTypes.Update"), Make("BudgetTypes.ToggleActive"),
            Make("BudgetTransactions.View"), Make("BudgetTransactions.Create"), Make("BudgetTransactions.Update"), Make("BudgetTransactions.Delete"),
            Make("BudgetTransactions.Submit"), Make("BudgetTransactions.Approve"), Make("BudgetTransactions.Post"),
            Make("BudgetTransactions.Cancel"), Make("BudgetTransactions.Reverse"),
            Make("BudgetItemAllocations.View"), Make("BudgetItemAllocations.Create"), Make("BudgetItemAllocations.Update"), Make("BudgetItemAllocations.Delete"),
            Make("Encumbrances.View"), Make("Encumbrances.Create"), Make("Encumbrances.Submit"), Make("Encumbrances.Approve"),
            Make("Encumbrances.Activate"), Make("Encumbrances.Release"), Make("Encumbrances.Suspend"), Make("Encumbrances.Close"), Make("Encumbrances.Cancel"), Make("Encumbrances.Reverse"),
            Make("Encumbrances.Update"), Make("Encumbrances.Delete"),

            // Parties & Procurement
            Make("Parties.View"), Make("Parties.Create"), Make("Parties.Update"),
            Make("PurchaseRequests.View"), Make("PurchaseRequests.Create"), Make("PurchaseRequests.Submit"),
            Make("PurchaseRequests.Approve"), Make("PurchaseRequests.Reject"), Make("PurchaseRequests.Cancel"),
            Make("RFQ.View"), Make("RFQ.Create"), Make("RFQ.Publish"), Make("RFQ.Complete"), Make("RFQ.Cancel"),
            Make("Quotations.View"), Make("Quotations.Create"),
            Make("PurchaseOrders.View"), Make("PurchaseOrders.Create"), Make("PurchaseOrders.Submit"),
            Make("PurchaseOrders.Approve"), Make("PurchaseOrders.Issue"), Make("PurchaseOrders.Close"), Make("PurchaseOrders.Cancel"),

            // Payments
            Make("BankAccounts.View"), Make("BankAccounts.Create"), Make("BankAccounts.Update"), Make("BankAccounts.Activate"), Make("BankAccounts.Deactivate"),
            Make("DisbursementRequests.View"), Make("DisbursementRequests.Create"), Make("DisbursementRequests.Submit"),
            Make("DisbursementRequests.Approve"), Make("DisbursementRequests.Reject"), Make("DisbursementRequests.Cancel"),
            Make("DisbursementRequests.Update"), Make("DisbursementRequests.CreateAccrual"),
            Make("Payments.View"), Make("Payments.Create"),
            Make("PaymentOrders.View"), Make("PaymentOrders.Create"), Make("PaymentOrders.Update"), Make("PaymentOrders.Submit"), Make("PaymentOrders.Approve"),
            Make("PaymentOrders.Reject"), Make("PaymentOrders.Cancel"), Make("PaymentOrders.SendToTreasury"), Make("PaymentOrders.Void"),

            // Committees
            Make("Committees.View"), Make("Committees.Create"), Make("Committees.Update"), Make("Committees.Activate"), Make("Committees.Deactivate"), Make("Committees.Dissolve"),
            Make("CommitteeMembers.View"), Make("CommitteeMembers.Add"), Make("CommitteeMembers.Remove"),
            Make("CommitteeAssignments.View"), Make("CommitteeAssignments.Create"), Make("CommitteeAssignments.Complete"),

            // Revenue
            Make("RevenueReceipts.View"), Make("RevenueReceipts.Create"), Make("RevenueReceipts.Approve"), Make("RevenueReceipts.Post"), Make("RevenueReceipts.Cancel"),

            // ReceiptVouchers
            Make("ReceiptVouchers.View"), Make("ReceiptVouchers.Create"), Make("ReceiptVouchers.Submit"), Make("ReceiptVouchers.Approve"), Make("ReceiptVouchers.Cancel"),

            // DepositSlips
            Make("DepositSlips.View"), Make("DepositSlips.Create"), Make("DepositSlips.Update"), Make("DepositSlips.Approve"),

            // Checks
            Make("Checks.View"), Make("Checks.Clear"), Make("Checks.Bounce"), Make("Checks.Replace"),

            // Assets
            Make("Assets.View"), Make("Assets.Create"), Make("Assets.Update"),
            Make("AssetGroups.View"), Make("AssetGroups.Create"), Make("AssetGroups.Update"),
            Make("AssetMovements.View"), Make("AssetMovements.Create"), Make("AssetMovements.Approve"),
            Make("AssetDisposals.View"), Make("AssetDisposals.Create"), Make("AssetDisposals.Approve"), Make("AssetDisposals.Post"),
            Make("AssetRevaluations.View"), Make("AssetRevaluations.Create"), Make("AssetRevaluations.Approve"), Make("AssetRevaluations.Post"),
            Make("AssetImpairments.View"), Make("AssetImpairments.Create"), Make("AssetImpairments.Approve"), Make("AssetImpairments.Post"),
            Make("Depreciation.View"), Make("Depreciation.Post"), Make("Depreciation.Reverse"),

            // Inventory
            Make("Items.View"), Make("Items.Create"), Make("Items.Update"),
            Make("ItemCategories.View"), Make("ItemCategories.Create"), Make("ItemCategories.Update"),
            Make("Units.View"), Make("Units.Create"), Make("Units.Update"),
            Make("Warehouses.View"), Make("Warehouses.Create"), Make("Warehouses.Update"),
            Make("Locations.View"), Make("Locations.Create"), Make("Locations.Update"),
            Make("GoodsReceiptNotes.View"), Make("GoodsReceiptNotes.Create"), Make("GoodsReceiptNotes.Approve"),
            Make("StockTakes.View"), Make("StockTakes.Create"), Make("StockTakes.Start"), Make("StockTakes.Complete"), Make("StockTakes.Approve"),
            Make("StockTransactions.View"),

            // Banking
            Make("BankStatements.View"), Make("BankStatements.Create"), Make("BankStatements.Import"),
            Make("BankReconciliation.View"), Make("BankReconciliation.Create"), Make("BankReconciliation.Approve"),

            // Approval
            Make("ApprovalRules.View"), Make("ApprovalRules.Manage"),

            // Workflow
            Make("WorkflowDefinitions.View"), Make("WorkflowDefinitions.Manage"),
            Make("WorkflowInstances.View"), Make("WorkflowInstances.Execute"),
            Make("WorkflowHistory.View"),

            // Security
            Make("Roles.View"), Make("Roles.Create"), Make("Roles.Update"), Make("Roles.Delete"),
            Make("Permissions.View"),

            // Users
            Make("Users.View"), Make("Users.Create"), Make("Users.Update"), Make("Users.Deactivate"),
            Make("Users.ManageSessions"), Make("Users.ResetLogin"), Make("Users.ManageRoles"),

            // System
            Make("System.Admin"),

            // Organization
            Make("OrgUnits.View"), Make("OrgUnits.Create"), Make("OrgUnits.Update"), Make("OrgUnits.Delete"),
            Make("Employees.View"), Make("Employees.Create"), Make("Employees.Update"), Make("Employees.Delete"),
            Make("CostCenters.View"), Make("CostCenters.Create"), Make("CostCenters.Update"), Make("CostCenters.Delete"),
            Make("Projects.View"), Make("Projects.Create"), Make("Projects.Update"), Make("Projects.Delete"),

            // Reports
            Make("Reports.Read"), Make("Reports.Export"),

            // Reporting (RPT-01..06)
            Make("Reporting.ViewBudgetExecution"), Make("Reporting.ExportReports"),
            Make("Reporting.ViewRevenueCollections"),
            Make("Reporting.ViewDisbursementRegister"),
            Make("Reporting.ViewAvailabilitySnapshot"),
            Make("Reporting.ViewTrialBalanceReport"),
            Make("Reporting.ViewFinancialStatements"),

            // Background Jobs
            Make("BackgroundJobs.View"), Make("BackgroundJobs.Manage"),

            // FinancialControl
            Make("FinancialControl.LapseYear"), Make("FinancialControl.ApproveFinalAccount"),

            // Notifications
            Make("Notifications.View"), Make("Notifications.MarkRead"), Make("Notifications.Delete"), Make("Notifications.Create"),
        ];
    }

    private static SecurityPermission Make(string code)
    {
        var parts = code.Split('.');
        var module = parts.Length > 1 ? parts[0] : "";
        var action = parts.Length > 1 ? string.Join('.', parts.Skip(1)) : code;
        return new SecurityPermission
        {
            Module = module,
            Action = action,
            Code = code,
            Name = code,
            IsSensitive = false,
        };
    }
}
