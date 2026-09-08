import { useMonthlyPlan } from '@/features/budgeting/hooks/useMonthlyPlan';
import { Button, Badge, Input } from '@/components/ui';
import { Copy, Save } from 'lucide-react';

const MONTH_NAMES = [
  'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
  'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر',
];

interface MonthlyPlanEditorProps {
  budgetItemId: number;
  appropriatedTotal: number;
}

export function MonthlyPlanEditor({ budgetItemId, appropriatedTotal }: MonthlyPlanEditorProps) {
  const { months, total, setMonth, save } = useMonthlyPlan(budgetItemId);
  const variance = appropriatedTotal - total;

  function handleCopy() {
    const header = MONTH_NAMES.join('\t');
    const values = months.map((m) => String(m)).join('\t');
    const tsv = `${header}\n${values}`;
    navigator.clipboard.writeText(tsv);
  }

  return (
    <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-semibold text-[var(--color-on-surface)]">الخطة الشهرية</h3>
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="sm" onClick={handleCopy}>
            <Copy size={14} className="ms-1" />
            نسخ
          </Button>
          <Button variant="primary" size="sm" onClick={save}>
            <Save size={14} className="ms-1" />
            حفظ
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-6 md:grid-cols-12 gap-2">
        {MONTH_NAMES.map((name, i) => (
          <div key={i} className="space-y-1">
            <label className="block text-[10px] text-[var(--color-on-surface-variant)] text-center">{name}</label>
            <Input
              type="number"
              value={months[i] || ''}
              onChange={(e) => setMonth(i, Number(e.target.value) || 0)}
              className="text-center font-mono text-xs"
              min="0"
              step="0.01"
            />
          </div>
        ))}
      </div>

      <div className="flex items-center justify-between mt-4 pt-3 border-t border-[var(--color-outline-variant)]">
        <div className="flex items-center gap-4 text-sm">
          <span className="text-[var(--color-on-surface-variant)]">المجموع</span>
          <span className="font-mono font-semibold">{total.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          {appropriatedTotal > 0 && (
            <>
              <span className="text-[var(--color-on-surface-variant)]">المخصص</span>
              <span className="font-mono">{appropriatedTotal.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
            </>
          )}
        </div>
        {appropriatedTotal > 0 && (
          <Badge variant={variance === 0 ? 'success' : variance > 0 ? 'warning' : 'danger'}>
            الفرق: {Math.abs(variance).toLocaleString('ar-EG', { minimumFractionDigits: 2 })}
          </Badge>
        )}
      </div>
    </div>
  );
}
