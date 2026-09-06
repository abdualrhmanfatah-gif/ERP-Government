import { notify } from '@/features/notifications/notify';
import { CheckCircle } from 'lucide-react';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { useReactivateUser } from '../hooks';

interface ReactivateUserDialogProps {
  open: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
}

export function ReactivateUserDialog({ open, onClose, userId, userName }: ReactivateUserDialogProps) {
  const reactivate = useReactivateUser();

  async function handleReactivate() {
    try {
      await reactivate.mutateAsync(userId);
      notify({ type: 'success', title: 'تم تنشيط المستخدم بنجاح' });
      onClose();
    } catch (err) {
      notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ' });
    }
  }

  return (
    <Dialog open={open} onClose={onClose} title="تنشيط المستخدم">
      <div className="space-y-4">
        <div className="flex items-center gap-3 text-[var(--color-success)]">
          <CheckCircle size={24} />
          <p className="text-[var(--color-on-surface-variant)]">
            هل أنت متأكد من تنشيط حساب "{userName}"؟ سيتمكن من تسجيل الدخول مرة أخرى.
          </p>
        </div>
        <div className="flex gap-2 justify-end">
          <Button variant="ghost" onClick={onClose}>إلغاء</Button>
          <Button variant="primary" onClick={handleReactivate} loading={reactivate.isPending}>
            تنشيط
          </Button>
        </div>
      </div>
    </Dialog>
  );
}
