import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCreateFiscalYear } from '../../hooks/useFiscalYears';

export default function FiscalYearCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateFiscalYear();
  const [errors, setErrors] = useState<Record<string, string>>({});

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors({});
    const form = new FormData(e.currentTarget);
    const name = form.get('name') as string;
    const startDate = form.get('startDate') as string;
    const endDate = form.get('endDate') as string;

    const newErrors: Record<string, string> = {};
    if (!name.trim()) newErrors.name = 'الاسم مطلوب';
    if (!startDate) newErrors.startDate = 'تاريخ البداية مطلوب';
    if (!endDate) newErrors.endDate = 'تاريخ النهاية مطلوب';
    if (startDate && endDate && new Date(startDate) >= new Date(endDate)) {
      newErrors.endDate = 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية';
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    createMutation.mutate(
      { name: name.trim(), startDate, endDate },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم إنشاء السنة المالية بنجاح' });
          navigate('/financial-settings/fiscal-years');
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      },
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/fiscal-years')} className="cursor-pointer">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">سنة مالية جديدة</h1>
      </div>

      <div className="rounded-xl border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">اسم السنة المالية *</label>
            <input
              id="name"
              name="name"
              type="text"
              required
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
            {errors.name && <p className="text-xs text-[var(--color-error)] mt-1">{errors.name}</p>}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="startDate" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">تاريخ البداية *</label>
              <input
                id="startDate"
                name="startDate"
                type="date"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              />
              {errors.startDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.startDate}</p>}
            </div>
            <div>
              <label htmlFor="endDate" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">تاريخ النهاية *</label>
              <input
                id="endDate"
                name="endDate"
                type="date"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              />
              {errors.endDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.endDate}</p>}
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="ghost" onClick={() => navigate('/financial-settings/fiscal-years')} className="cursor-pointer">
              إلغاء
            </Button>
            <Button type="submit" disabled={createMutation.isPending} className="cursor-pointer">
              {createMutation.isPending ? 'جاري الإنشاء...' : 'إنشاء'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
