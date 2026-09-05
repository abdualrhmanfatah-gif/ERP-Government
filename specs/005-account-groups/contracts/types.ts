// contracts/types.ts — Account Groups — 005-account-groups
// Mirrors src/Application/Accounting/Common/AccountingDtos.cs + new detail/toggle DTOs
// Frontend must use generated web-api-client.ts or keep this in sync; OpenAPI is source of truth.

export type AccountGroupType = 'Asset' | 'Liability' | 'Equity' | 'Revenue' | 'Expense';
export type NormalBalanceType = 'Debit' | 'Credit';

export interface AncestorRef {
  id: number;
  code: string;
  name: string;
}

export interface AccountGroupTreeItem {
  id: number;
  code: string;
  name: string;
  type: AccountGroupType;
  normalBalance: NormalBalanceType;
  description?: string | null;
  parentId?: number | null;
  level: number; // 1-5
  isActive: boolean;
  rowVersion: string; // base64
  ancestorPath?: AncestorRef[] | null; // flat mode
  children?: AccountGroupTreeItem[]; // tree mode
}

export type AccountGroupListItem = Omit<AccountGroupTreeItem, 'children'> & {
  children?: never;
};

export interface PaginatedAccountGroupsResponse {
  mode: 'tree' | 'flat';
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: (AccountGroupTreeItem | AccountGroupListItem)[];
}

export interface LinkedAccountRef {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
  isPostable: boolean;
}

export interface AuditEntry {
  id: number;
  action: 'Create' | 'Update' | 'Deactivate' | 'Activate';
  userId?: number | null;
  userName?: string | null;
  timestamp: string; // ISO
  fieldChanges?: string | null; // JSON {field:{Old,New}}
  changeSummary?: string | null;
  ipAddress?: string | null;
}

export interface AccountGroupDetailResponse {
  group: AccountGroupListItem;
  children: AccountGroupListItem[];
  accounts: LinkedAccountRef[];
  audit: AuditEntry[];
}

export interface CreateAccountGroupRequest {
  code: string; // 1-20 trimmed, permanent unique case-insensitive
  name: string; // 1-200 trimmed
  type: AccountGroupType;
  normalBalance: NormalBalanceType;
  description?: string | null; // 0-500
  parentId?: number | null;
}

export interface UpdateAccountGroupRequest {
  id: number;
  name: string;
  type: AccountGroupType;
  normalBalance: NormalBalanceType;
  description?: string | null;
  parentId?: number | null;
  rowVersion: string;
}

export interface ToggleActiveRequest {
  rowVersion: string;
  isActive: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  current?: AccountGroupListItem; // 409 only
}
