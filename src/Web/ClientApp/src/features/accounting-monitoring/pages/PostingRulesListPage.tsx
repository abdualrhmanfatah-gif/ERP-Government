import { usePostingRules, useDeletePostingRule } from '../hooks/usePostingRules';
import { Button, Badge, Loading } from '@/components/ui';

export function PostingRulesListPage() {
  const { data: rules, isLoading } = usePostingRules();
  const deleteMutation = useDeletePostingRule();

  const handleDelete = (id: number, name: string) => {
    if (confirm(`هل أنت متأكد من حذف قاعدة "${name}"؟`)) {
      deleteMutation.mutate(id);
    }
  };

  return (
    <div dir="rtl" className="p-6">
      <h1 className="text-2xl font-bold mb-6" style={{ color: 'var(--color-onSurface)' }}>
        قواعد الترحيل
      </h1>

      {isLoading ? (
        <Loading />
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse" style={{ color: 'var(--color-onSurface)' }}>
            <thead>
              <tr style={{ backgroundColor: 'var(--color-surface)' }}>
                <th className="border p-2 text-right">الاسم</th>
                <th className="border p-2 text-right">نوع الحدث</th>
                <th className="border p-2 text-right">اليومية</th>
                <th className="border p-2 text-right">الأولوية</th>
                <th className="border p-2 text-right">نشط</th>
                <th className="border p-2 text-right">البنود</th>
                <th className="border p-2 text-right">إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {rules?.map((rule) => (
                <tr key={rule.id}>
                  <td className="border p-2">{rule.name}</td>
                  <td className="border p-2">{rule.eventType}</td>
                  <td className="border p-2">{rule.journalName}</td>
                  <td className="border p-2">{rule.priority}</td>
                  <td className="border p-2">
                    <Badge variant={rule.isActive ? 'success' : 'default'}>
                      {rule.isActive ? 'نشط' : 'غير نشط'}
                    </Badge>
                  </td>
                  <td className="border p-2">{rule.lines?.length ?? 0}</td>
                  <td className="border p-2">
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleDelete(rule.id, rule.name)}
                    >
                      حذف
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
