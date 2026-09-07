import { usePostingRules, useDeletePostingRule } from '../hooks/usePostingRules';
import { Button, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';

type RuleRow = NonNullable<ReturnType<typeof usePostingRules>['data']>[number];

export function PostingRulesListPage() {
  const { data: rules, isLoading } = usePostingRules();
  const deleteMutation = useDeletePostingRule();

  const handleDelete = (id: number, name: string) => {
    if (confirm(`هل أنت متأكد من حذف قاعدة "${name}"؟`)) {
      deleteMutation.mutate(id);
    }
  };

  const columns: DataGridColumn<RuleRow>[] = [
    { key: 'name', header: 'الاسم' },
    { key: 'eventType', header: 'نوع الحدث' },
    { key: 'journalName', header: 'اليومية' },
    { key: 'priority', header: 'الأولوية' },
    {
      key: 'isActive',
      header: 'نشط',
      render: (rule) => (
        <Badge variant={rule.isActive ? 'success' : 'default'}>
          {rule.isActive ? 'نشط' : 'غير نشط'}
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
        <Button
          variant="destructive"
          size="sm"
          onClick={() => handleDelete(rule.id, rule.name)}
        >
          حذف
        </Button>
      ),
    },
  ];

  return (
    <div dir="rtl" className="p-6">
      <h1 className="text-2xl font-bold mb-6 text-[var(--color-on-surface)]">
        قواعد الترحيل
      </h1>

      <DataGrid
        columns={columns}
        data={rules ?? []}
        loading={isLoading}
        emptyMessage="لا توجد قواعد ترحيل"
        rowKey={(rule) => rule.id}
      />
    </div>
  );
}
