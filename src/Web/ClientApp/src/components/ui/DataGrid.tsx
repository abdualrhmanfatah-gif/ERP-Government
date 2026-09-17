import { useMemo } from 'react';
import {
  useTable,
  createCoreRowModel,
  flexRender,
  type ColumnDef,
  type SortingState,
  type ColumnFiltersState,
  type PaginationState,
} from '@tanstack/react-table';
import { cn } from '@/lib/utils';
import { Loading } from './Loading';
import { EmptyState } from './EmptyState';
import { ErrorState } from './ErrorState';
import { Pagination } from './Pagination';
import { MobileCard, MobileCardField } from './MobileCard';

export interface DataGridColumn<T> {
  id?: string;
  key?: string;
  header: string;
  accessorKey?: keyof T;
  accessorFn?: (row: T) => unknown;
  sortable?: boolean;
  filterable?: boolean;
  align?: 'left' | 'right' | 'center';
  width?: number;
  cell?: (row: T) => React.ReactNode;
  render?: (row: T) => React.ReactNode;
  headerClassName?: string;
  cellClassName?: string;
}

interface DataGridProps<T> {
  columns: DataGridColumn<T>[];
  data: T[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
  emptyMessage?: string;
  rowKey: (row: T) => string | number;
  onRowClick?: (row: T) => void;
  selectedRowKey?: string | number | null;
  striped?: boolean;
  pagination?: PaginationState;
  totalItems?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  sorting?: SortingState;
  onSortingChange?: (sorting: SortingState) => void;
  columnFilters?: ColumnFiltersState;
  onColumnFiltersChange?: (filters: ColumnFiltersState) => void;
}

const alignMap: Record<'left' | 'right' | 'center', string> = {
  left: 'text-start',
  right: 'text-end',
  center: 'text-center',
};

export function DataGrid<T>({
  columns,
  data,
  loading = false,
  error,
  onRetry,
  emptyMessage,
  rowKey,
  onRowClick,
  selectedRowKey,
  striped = true,
  pagination,
  totalItems = 0,
  onPageChange,
  onPageSizeChange,
  sorting = [],
  onSortingChange,
  columnFilters = [],
  onColumnFiltersChange,
}: DataGridProps<T>) {
  void onColumnFiltersChange;
  void onPageSizeChange;
  const tableColumns = useMemo(
    () =>
      columns.map((col) => {
        const colId = col.id ?? col.key ?? (col.accessorKey as string | undefined) ?? String(col.header);
        const cellRenderer = col.cell ?? col.render;
        return {
          id: colId,
          header: col.header,
          accessorKey: (col.accessorKey ?? col.key) as string | undefined,
          accessorFn: col.accessorFn as ((row: T) => unknown) | undefined,
          enableSorting: col.sortable ?? false,
          enableColumnFilter: col.filterable ?? false,
          size: col.width,
          meta: {
            align: col.align ?? 'left',
            headerClassName: col.headerClassName,
            cellClassName: col.cellClassName,
          },
          ...(cellRenderer
            ? { cell: (info: any) => cellRenderer(info.row.original) }
            : {}),
        } as unknown as ColumnDef<any, any>;
      }),
    [columns]
  );

  const table = useTable({
    data: data as any,
    columns: tableColumns as any,
    getCoreRowModel: createCoreRowModel(),
    manualPagination: true,
    manualSorting: true,
    manualFiltering: true,
    pageCount: pagination ? Math.ceil(totalItems / pagination.pageSize) : undefined,
    state: {
      sorting,
      columnFilters,
      pagination: pagination as any,
    } as any,
  } as any);

  if (loading) return <div aria-live="polite"><Loading /></div>;
  if (error) return <div aria-live="assertive"><ErrorState message={error} onRetry={onRetry} /></div>;
  if (data.length === 0) return <EmptyState message={emptyMessage} />;

  const headerGroups = table.getHeaderGroups();
  const rows = table.getRowModel().rows;

  return (
    <div className="flex flex-col gap-4">
       {/* Desktop table view */}
       <div className="hidden md:block overflow-x-auto border border-[var(--color-primary-container)] rounded-xl">
        <table
          role="table"
          className="w-full border-collapse text-sm leading-relaxed"
        >
          {emptyMessage && (
            <caption className="sr-only">{emptyMessage}</caption>
          )}
          <thead>
            {headerGroups.map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => {
                  const col = columns.find((c) => (c.id ?? c.key) === header.id);
                  const align = col?.align ?? 'left';
                  const canSort = col?.sortable ?? false;

                  const handleSort = canSort && onSortingChange
                    ? () => {
                        const currentDir = sorting.find(
                          (s) => s.id === header.id
                        )?.desc;
                        const nextSorting: SortingState =
                          currentDir === undefined
                            ? [{ id: header.id, desc: false }]
                            : currentDir === false
                              ? [{ id: header.id, desc: true }]
                              : [];
                        onSortingChange(nextSorting);
                      }
                    : undefined;

                  const handleSortKeyDown = (e: React.KeyboardEvent) => {
                    if (handleSort && (e.key === 'Enter' || e.key === ' ')) {
                      e.preventDefault();
                      handleSort();
                    }
                  };

                  const currentSort = sorting.find((s) => s.id === header.id);
                  return (
                    <th
                      key={header.id}
                      scope="col"
                      aria-sort={
                        currentSort
                          ? currentSort.desc
                            ? 'descending'
                            : 'ascending'
                          : undefined
                      }
                      className={cn(
                         'px-3 py-2.5 font-semibold text-sm uppercase tracking-wide text-[var(--color-on-primary)] bg-[var(--color-primary)] border-e border-b border-[var(--color-primary-container)] whitespace-nowrap',
                         alignMap[align],
                         canSort && 'cursor-pointer select-none hover:bg-[var(--color-primary-container)] transition-colors',
                        col?.headerClassName
                      )}
                      style={{ width: col?.width ?? undefined }}
                      tabIndex={canSort ? 0 : undefined}
                      onClick={handleSort}
                      onKeyDown={handleSortKeyDown}
                    >
                      <div className="flex items-center gap-1">
                        {flexRender(
                          header.column.columnDef.header,
                          header.getContext()
                        )}
                        {canSort && (
                          <span aria-hidden="true" className="text-[var(--color-on-surface-variant)]">
                            {currentSort?.desc
                              ? ' ▼'
                              : currentSort
                                ? ' ▲'
                                : ' ↕'}
                          </span>
                        )}
                      </div>
                    </th>
                  );
                })}
              </tr>
            ))}
          </thead>
          <tbody>
            {rows.map((row, idx) => {
              const rowKeyValue = rowKey(row.original as T);
              const isSelected = selectedRowKey !== undefined && selectedRowKey === rowKeyValue;
              return (
                <tr
                  key={rowKeyValue}
                  role={onRowClick ? 'button' : undefined}
                  aria-selected={isSelected || undefined}
                  onClick={onRowClick ? () => onRowClick(row.original as T) : undefined}
                  onKeyDown={onRowClick ? (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      onRowClick(row.original as T);
                    }
                  } : undefined}
                  tabIndex={onRowClick ? 0 : undefined}
                  className={cn(
                     striped && idx % 2 === 1 ? 'bg-[color-mix(in_srgb,var(--color-primary-container)_4%,transparent)]' : 'bg-[var(--color-surface-container-lowest)]',
                     isSelected && 'bg-[color-mix(in_srgb,var(--color-primary-container)_10%,transparent)] border-e-3 border-e-[var(--color-primary)] font-semibold',
                     onRowClick && 'cursor-pointer',
                     'transition-all duration-150 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_5%,transparent)]'
                  )}
                >
                  {row.getAllCells().map((cell) => {
                    const col = columns.find((c) => (c.id ?? c.key) === cell.column.id);
                    const align = col?.align ?? 'left';

                    return (
                      <td
                        key={cell.column.id}
                        className={cn(
                           'px-3 py-1.5 tabular-nums',
                          alignMap[align],
                          col?.cellClassName
                        )}
                      >
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext()
                        )}
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Mobile card view */}
      <div className="md:hidden flex flex-col gap-3">
        {rows.map((row) => {
          const rowKeyValue = rowKey(row.original as T);
          const isSelected = selectedRowKey !== undefined && selectedRowKey === rowKeyValue;
          return (
            <MobileCard
              key={rowKeyValue}
              onClick={onRowClick ? () => onRowClick(row.original as T) : undefined}
              onKeyDown={onRowClick ? (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onRowClick(row.original as T);
                }
              } : undefined}
              tabIndex={onRowClick ? 0 : undefined}
              role={onRowClick ? 'button' : undefined}
              aria-selected={isSelected || undefined}
            >
              <div className="grid grid-cols-2 gap-3">
                {row.getAllCells().map((cell) => {
                  const col = columns.find((c) => (c.id ?? c.key) === cell.column.id);
                  return (
                    <MobileCardField
                      key={cell.column.id}
                      label={col?.header ?? ''}
                      value={flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    />
                  );
                })}
              </div>
            </MobileCard>
          );
        })}
      </div>

      {pagination && onPageChange && (
        <Pagination
          page={(pagination as any).pageIndex !== undefined ? (pagination as any).pageIndex + 1 : 1}
          pageSize={pagination.pageSize}
          total={totalItems}
          onChange={(p: number) => onPageChange(p - 1)}
        />
      )}
    </div>
  );
}
