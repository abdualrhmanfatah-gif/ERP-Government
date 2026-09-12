import { useState } from 'react';
import { usePostingRules, useDeletePostingRule } from '../hooks/usePostingRules';
import { Page, Button, Badge, ConfirmDialog, Sheet } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import type { PostingRuleDto, PostingRuleLineDto } from '../shared/types';

type RuleRow = NonNullable<ReturnType<typeof usePostingRules>['data']>[number];

export function PostingRulesListPage() {
  const { data: rules, isLoading } = usePostingRules();
  const deleteMutation = useDeletePostingRule();
  const [deleteTarget, setDeleteTarget] = useState<{ id: number; name: string } | null>(null);
  const [linesTarget, setLinesTarget] = useState<PostingRuleDto | null>(null);

  const handleDelete = () => {
    if (!deleteTarget) return;
    deleteMutation.mutate(deleteTarget.id, {
      onSettled: () => setDeleteTarget(null),
    });
  };

  const columns: DataGridColumn<RuleRow>[] = [
    { key: 'name', header: 'الاسم' },
    { key: 'eventType', header: 'نوع الحدث' },
    { key: 'journalName', header: 'اليومية' },
    { key: 'priority', header: 'الأولوية' },
    {
      key: 'isActive',
      header: 'الحالة',
      render: (rule) => (
        <Badge variant={rule.isActive ? 'success' : 'default'}>
          {getActiveStatusLabel(rule.isActive)}
        </Badge>
      ),
    },
    {
      key: 'linesCount',
      header: 'البنود',
      accessorFn: (rule) => rule.lines?.length ?? 0,
    },
    {
      key: 'actions',
      header: 'إجراءات',
      render: (rule) => (
        <button
          type="button"
          className="text-red-600 underline cursor-pointer bg-transparent border-none p-0 text-sm hover:text-red-800"
          onClick={() => setDeleteTarget({ id: rule.id, name: rule.name })}
        >
          حذف
        </button>
      ),
    },
  ];

  return (
    <Page title="قواعد الترحيل">
      <DataGrid
        columns={columns}
        data={rules ?? []}
        loading={isLoading}
        emptyMessage="لا توجد قواعد ترحيل"
        rowKey={(rule) => rule.id}
        onRowClick={(rule) => setLinesTarget(rule)}
      />
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        message={`هل أنت متأكد من حذف قاعدة "${deleteTarget?.name ?? ''}"؟`}
        confirmLabel="حذف"
        destructive
        loading={deleteMutation.isPending}
      />
      <Sheet
        open={!!linesTarget}
        onClose={() => setLinesTarget(null)}
        title={`بنود قاعدة "${linesTarget?.name ?? ''}"`}
      >
        {linesTarget?.lines && linesTarget.lines.length > 0 ? (
          <div className="flex flex-col gap-3">
            {linesTarget.lines
              .sort((a, b) => a.sequence - b.sequence)
              .map((line) => (
                <PostingRuleLineCard key={line.id} line={line} />
              ))}
          </div>
        ) : (
          <p className="text-[var(--color-on-surface-variant)] text-sm">
            لا توجد بنود معرّفة لهذه القاعدة.
          </p>
        )}
      </Sheet>
    </Page>
  );
}

function PostingRuleLineCard({ line }: { line: PostingRuleLineDto }) {
  return (
    <div className="border border-[var(--color-border-container)] rounded-lg p-4 flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-[var(--color-on-surface)]">
          البند {line.sequence}
        </span>
        <Badge variant={line.isActive ? 'success' : 'default'}>
          {line.isActive ? 'نشط' : 'غير نشط'}
        </Badge>
      </div>
      <div className="grid grid-cols-2 gap-2 text-sm">
        <div>
          <span className="text-[var(--color-on-surface-variant)]">الحساب: </span>
          <span className="text-[var(--color-on-surface)]">{line.fixedAccountCode ?? '—'}</span>
        </div>
        <div>
          <span className="text-[var(--color-on-surface-variant)]">النوع: </span>
          <Badge variant={line.debitOrCredit === 'Debit' ? 'warning' : 'info'}>
            {line.debitOrCredit === 'Debit' ? 'مدين' : 'دائن'}
          </Badge>
        </div>
        <div>
          <span className="text-[var(--color-on-surface-variant)]">المصدر: </span>
          <span className="text-[var(--color-on-surface)]">
            {line.accountSource === 'FixedAccount' ? 'حساب ثابت' : 'بُعد الحدث'}
          </span>
        </div>
        <div>
          <span className="text-[var(--color-on-surface-variant)]">المبلغ: </span>
          <span className="text-[var(--color-on-surface)]">
            {line.amountSource === 'EventAmount' ? 'مبلغ الحدث' : 'مبلغ ثابت'}
          </span>
        </div>
      </div>
      {(line.fundDimensionRequired || line.costCenterDimensionRequired || line.projectDimensionRequired) && (
        <div className="flex gap-2 flex-wrap mt-1">
          {line.fundDimensionRequired && <Badge variant="outline">صندوق</Badge>}
          {line.costCenterDimensionRequired && <Badge variant="outline">مركز تكلفة</Badge>}
          {line.projectDimensionRequired && <Badge variant="outline">مشروع</Badge>}
        </div>
      )}
    </div>
  );
}
