import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { useReverseMove } from '../hooks/useMoves';

interface ReverseDialogProps {
  moveId: number;
  rowVersion: string;
  onClose: () => void;
}

export function ReverseDialog({ moveId, rowVersion, onClose }: ReverseDialogProps) {
  const [reason, setReason] = useState('');
  const reverse = useReverseMove();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!reason.trim()) return;
    try {
      await reverse.mutateAsync({ id: moveId, reversalReason: reason.trim(), rowVersion });
      onClose();
    } catch {
      // keep dialog open to show error
    }
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="عكس القيد"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="bg-[var(--color-surface-container-lowest)] rounded-lg border border-[var(--color-border-container)] p-4 w-full max-w-md space-y-4"
        onClick={(e) => e.stopPropagation()}
      >
        <h3 className="text-sm font-semibold text-[var(--color-primary)]">عكس القيد</h3>
        <p className="text-xs text-[var(--color-outline)]">يجب إدخال سبب العكس (حتى 500 حرف). سيتم إنشاء قيد عكسي مرحّل فورًا.</p>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div>
            <label htmlFor="reversalReason" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
              سبب العكس *
            </label>
            <textarea
              id="reversalReason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              maxLength={500}
              rows={3}
              required
              aria-required="true"
              className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] text-sm focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
              placeholder="خطأ في التسجيل"
            />
            <p className="text-xs text-[var(--color-outline)] mt-1 tabular-nums">{reason.length}/500</p>
          </div>

          {reverse.isError ? (
            <p role="alert" className="text-xs text-[var(--color-error)]">
              {(reverse.error as Error)?.message ?? 'فشل عكس القيد'}
            </p>
          ) : null}

          <div className="flex justify-end gap-2">
            <Button variant="outline" size="sm" onClick={onClose}>
              إلغاء
            </Button>
            <Button type="submit" variant="destructive" size="sm" disabled={reverse.isPending || !reason.trim()} loading={reverse.isPending}>
              {reverse.isPending ? 'جاري العكس...' : 'تأكيد العكس'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
