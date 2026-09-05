export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ApiError {
  message: string;
  status?: number;
  details?: string;
}

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface UserPermissions {
  permissions: string[];
}

export interface PaginationState {
  page: number;
  pageSize: number;
}

export interface SortingState {
  id: string;
  desc: boolean;
}

export interface ColumnFilterState {
  id: string;
  value: string;
}

export interface DataGridColumn<T> {
  id: string;
  header: string;
  accessorKey?: keyof T;
  accessorFn?: (row: T) => unknown;
  sortable?: boolean;
  filterable?: boolean;
  align?: 'left' | 'right' | 'center';
  width?: number;
  cell?: (row: T) => React.ReactNode;
  headerClassName?: string;
  cellClassName?: string;
}
