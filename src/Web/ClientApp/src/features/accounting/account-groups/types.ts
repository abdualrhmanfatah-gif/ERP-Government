export type AccountGroupType = 'Asset' | 'Liability' | 'Equity' | 'Revenue' | 'Expense';
export type NormalBalanceType = 'Debit' | 'Credit';

export interface AncestorRef {
  id: number;
  code: string;
  name: string;
}

export interface AccountGroupDto {
  id: number;
  code: string;
  name: string;
  type: AccountGroupType;
  normalBalance: NormalBalanceType;
  description?: string | null;
  parentId?: number | null;
  level: number;
  isActive: boolean;
  rowVersion: string;
  ancestorPath?: AncestorRef[] | null;
  children?: AccountGroupDto[];
}

export interface PaginatedAccountGroupsResponse {
  mode: 'tree' | 'flat';
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: AccountGroupDto[];
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
  action: string;
  userId?: number | null;
  userName?: string | null;
  timestamp: string;
  fieldChanges?: string | null;
  changeSummary?: string | null;
  ipAddress?: string | null;
}

export interface AccountGroupDetailResponse {
  group: AccountGroupDto;
  children: AccountGroupDto[];
  accounts: LinkedAccountRef[];
  audit: AuditEntry[];
}

export interface CreateAccountGroupRequest {
  code: string;
  name: string;
  type: AccountGroupType;
  normalBalance: NormalBalanceType;
  description?: string | null;
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
