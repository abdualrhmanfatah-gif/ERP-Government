namespace ERP_Government.Application.Common.Errors;

public static class ErrorCodes
{
    public static class Request
    {
        public const string ValidationFailed = "Request.ValidationFailed";
        public const string NotFound = "Request.NotFound";
        public const string Forbidden = "Request.Forbidden";
        public const string Unauthorized = "Request.Unauthorized";
        public const string ConcurrencyConflict = "Request.ConcurrencyConflict";
        public const string RateLimitExceeded = "Request.RateLimitExceeded";
        public const string InternalError = "Request.InternalError";
        public const string ServiceUnavailable = "Request.ServiceUnavailable";
    }

    public static class Budgets
    {
        public const string InsufficientAvailability = "Budgets.InsufficientAvailability";
        public const string OverrunBlocked = "Budgets.OverrunBlocked";
        public const string FiscalYearClosed = "Budgets.FiscalYearClosed";
        public const string FiscalPeriodClosed = "Budgets.FiscalPeriodClosed";
        public const string BudgetNotFound = "Budgets.BudgetNotFound";
        public const string ClassificationNotFound = "Budgets.ClassificationNotFound";
        public const string FundNotFound = "Budgets.FundNotFound";
        public const string BudgetTypeNotFound = "Budgets.BudgetTypeNotFound";
        public const string AllocationNotFound = "Budgets.AllocationNotFound";
        public const string TransactionNotFound = "Budgets.TransactionNotFound";
        public const string EncumbranceNotFound = "Budgets.EncumbranceNotFound";
        public const string AvailabilityNotFound = "Budgets.AvailabilityNotFound";
    }

    public static class Parties
    {
        public const string DuplicateCode = "Parties.DuplicateCode";
        public const string NotFound = "Parties.NotFound";
        public const string DeactivationBlocked = "Parties.DeactivationBlocked";
    }

    public static class Payments
    {
        public const string InsufficientBalance = "Payments.InsufficientBalance";
        public const string InvalidAccount = "Payments.InvalidAccount";
        public const string PaymentNotFound = "Payments.PaymentNotFound";
        public const string ApprovalRequired = "Payments.ApprovalRequired";
        public const string DisbursementRequestNotFound = "Payments.DisbursementRequestNotFound";
        public const string PaymentOrderNotFound = "Payments.PaymentOrderNotFound";
        public const string PaymentOrderTotalsNotFound = "Payments.PaymentOrderTotalsNotFound";
        public const string BankAccountNotFound = "Payments.BankAccountNotFound";
    }

    public static class Accounting
    {
        public const string EntryNotBalanced = "Accounting.EntryNotBalanced";
        public const string PeriodClosed = "Accounting.PeriodClosed";
        public const string AccountNotPostable = "Accounting.AccountNotPostable";
        public const string DuplicateEntryNumber = "Accounting.DuplicateEntryNumber";
        public const string AccountGroupNotFound = "Accounting.AccountGroupNotFound";
        public const string AccountNotFound = "Accounting.AccountNotFound";
        public const string JournalNotFound = "Accounting.JournalNotFound";
        public const string JournalEntryNotFound = "Accounting.JournalEntryNotFound";
        public const string RecurringEntryNotFound = "Accounting.RecurringEntryNotFound";
    }

    public static class Inventory
    {
        public const string ItemNotFound = "Inventory.ItemNotFound";
        public const string DuplicateItemCode = "Inventory.DuplicateItemCode";
        public const string InsufficientStock = "Inventory.InsufficientStock";
        public const string WarehouseNotFound = "Inventory.WarehouseNotFound";
        public const string UnitNotFound = "Inventory.UnitNotFound";
        public const string ItemCategoryNotFound = "Inventory.ItemCategoryNotFound";
        public const string LocationNotFound = "Inventory.LocationNotFound";
        public const string LocationInactive = "Inventory.LocationInactive";
        public const string DuplicateLocationCode = "Inventory.DuplicateLocationCode";
        public const string LocationParentNotFound = "Inventory.LocationParentNotFound";
        public const string LocationSelfParent = "Inventory.LocationSelfParent";
        public const string LocationCycleDetected = "Inventory.LocationCycleDetected";
        public const string LocationParentInactive = "Inventory.LocationParentInactive";
        public const string LocationStateUnchanged = "Inventory.LocationStateUnchanged";
        public const string LocationDeactivationBlocked = "Inventory.LocationDeactivationBlocked";
    }

    public static class Procurement
    {
        public const string RfqNotFound = "Procurement.RfqNotFound";
        public const string QuotationNotFound = "Procurement.QuotationNotFound";
        public const string InvalidQuotationStatus = "Procurement.InvalidQuotationStatus";
    }

    public static class Revenue
    {
        public const string ReceiptNotFound = "Revenue.ReceiptNotFound";
        public const string InvalidReceiptStatus = "Revenue.InvalidReceiptStatus";
    }

    public static class Banking
    {
        public const string TransactionNotFound = "Banking.TransactionNotFound";
        public const string ReconciliationFailed = "Banking.ReconciliationFailed";
        public const string StatementNotFound = "Banking.StatementNotFound";
        public const string BankReconciliationNotFound = "Banking.BankReconciliationNotFound";
    }

    public static class Security
    {
        public const string UserNotFound = "Security.UserNotFound";
        public const string RoleNotFound = "Security.RoleNotFound";
        public const string InsufficientPermissions = "Security.InsufficientPermissions";
        public const string PermissionNotFound = "Security.PermissionNotFound";
    }

    public static class Organization
    {
        public const string DepartmentNotFound = "Organization.DepartmentNotFound";
        public const string PositionNotFound = "Organization.PositionNotFound";
        public const string ProjectNotFound = "Organization.ProjectNotFound";
        public const string OrgUnitNotFound = "Organization.OrgUnitNotFound";
        public const string EmployeeNotFound = "Organization.EmployeeNotFound";
        public const string CostCenterNotFound = "Organization.CostCenterNotFound";
    }

    public static class Workflow
    {
        public const string DefinitionNotFound = "Workflow.DefinitionNotFound";
        public const string InstanceNotFound = "Workflow.InstanceNotFound";
        public const string InstanceNotInProgress = "Workflow.InstanceNotInProgress";
        public const string StepNotFound = "Workflow.StepNotFound";
        public const string ActiveInstancesExist = "Workflow.ActiveInstancesExist";
        public const string NoApprovalRules = "Workflow.NoApprovalRules";
        public const string UserMissingRequiredRole = "Workflow.UserMissingRequiredRole";
        public const string IdentityRequired = "Workflow.IdentityRequired";
        public const string NoActiveSteps = "Workflow.NoActiveSteps";
        public const string ActiveInstanceExists = "Workflow.ActiveInstanceExists";
        public const string StepNotApproval = "Workflow.StepNotApproval";
    }

    public static class SecurityApprovalRules
    {
        public const string NotFound = "Security.ApprovalRuleNotFound";
        public const string RoleNotFound = "Security.ApprovalRoleNotFound";
        public const string DuplicateRule = "Security.DuplicateApprovalRule";
    }

    public static class Committees
    {
        public const string CommitteeNotFound = "Committees.CommitteeNotFound";
        public const string MemberNotFound = "Committees.MemberNotFound";
        public const string AssignmentNotFound = "Committees.AssignmentNotFound";
    }

    public static class Assets
    {
        public const string AssetGroupNotFound = "Assets.AssetGroupNotFound";
        public const string DuplicateAssetGroupCode = "Assets.DuplicateAssetGroupCode";
        public const string DeactivationBlocked = "Assets.DeactivationBlocked";
        public const string AlreadyActive = "Assets.AlreadyActive";
        public const string AlreadyInactive = "Assets.AlreadyInactive";
        public const string InactiveGroup = "Assets.InactiveGroup";
        public const string CycleDetected = "Assets.CycleDetected";
        public const string MaxDepthExceeded = "Assets.MaxDepthExceeded";
        public const string SelfParent = "Assets.SelfParent";
        public const string AssetNotFound = "Assets.AssetNotFound";
        public const string DuplicateAssetTag = "Assets.DuplicateAssetTag";
        public const string DuplicateBarcode = "Assets.DuplicateBarcode";
        public const string DuplicateSerialNumber = "Assets.DuplicateSerialNumber";
        public const string InvalidStatusTransition = "Assets.InvalidStatusTransition";
        public const string FieldLockedAfterActivation = "Assets.FieldLockedAfterActivation";
        public const string CannotDeactivate = "Assets.CannotDeactivate";
        public const string DuplicateAttributeCode = "Assets.DuplicateAttributeCode";
        public const string AttributeDefinitionNotFound = "Assets.AttributeDefinitionNotFound";
        public const string BindingNotFound = "Assets.BindingNotFound";
        public const string NoEligibleAssets = "Assets.NoEligibleAssets";
        public const string NoSchedulesToPost = "Assets.NoSchedulesToPost";
        public const string DepreciationRunNotFound = "Assets.DepreciationRunNotFound";
        public const string DuplicateDepreciationRun = "Assets.DuplicateDepreciationRun";
        public const string DuplicateDisposal = "Assets.DuplicateDisposal";
        public const string PostingGateFailed = "Assets.PostingGateFailed";
        public const string DepreciationTemplateNotFound = "Assets.DepreciationTemplateNotFound";
        public const string CountNotFound = "Assets.CountNotFound";
        public const string CountLineNotFound = "Assets.CountLineNotFound";
        public const string CountLinesPending = "Assets.CountLinesPending";
        public const string InvalidCountStatus = "Assets.InvalidCountStatus";
        public const string InvalidFoundState = "Assets.InvalidFoundState";
        public const string DuplicateCountLine = "Assets.DuplicateCountLine";
        public const string InvalidAttributeValue = "Assets.InvalidAttributeValue";
        public const string ReversalAlreadyExists = "Assets.ReversalAlreadyExists";
        public const string TransferNotFound = "Assets.TransferNotFound";
        public const string InvalidTransferDestination = "Assets.InvalidTransferDestination";
        public const string TransferSourceChanged = "Assets.TransferSourceChanged";
    }

    public static class FinancialSettings
    {
        public const string CurrencyNotFound = "FinancialSettings.CurrencyNotFound";
        public const string ExchangeRateNotFound = "FinancialSettings.ExchangeRateNotFound";
        public const string SequenceNotFound = "FinancialSettings.SequenceNotFound";
        public const string FiscalYearNotFound = "FinancialSettings.FiscalYearNotFound";
        public const string FiscalYearPeriodNotFound = "FinancialSettings.FiscalYearPeriodNotFound";
        public const string FiscalPeriodNotFound = "FinancialSettings.FiscalPeriodNotFound";
        public const string ClosingEntryNotFound = "FinancialSettings.ClosingEntryNotFound";
        public const string DocumentSequenceNotFound = "FinancialSettings.DocumentSequenceNotFound";
    }
}
