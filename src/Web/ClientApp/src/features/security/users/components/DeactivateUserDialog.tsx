import { notify } from '@/features/notifications/notify';
import { AlertTriangle } from 'lucide-react';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { useDeactivateUser } from '../hooks';

interface DeactivateUserDialogProps {
  open: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
}

export function DeactivateUserDialog({ open, onClose, userId, userName }: DeactivateUserDialogProps) {
  const deactivate = useDeactivateUser();

  async function handleDeactivate() {
    try {
      await deactivate.mutateAsync(userId);
      notify({ type: 'success', title: 'تم تعطيل المستخدم بنجاح' });
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ';
      if (message.includes('self') || message.includes('own account')) {
        notify({ type: 'error', title: 'لا يمكن تعطيل حسابك الخاص' });
      } else {
        notify({ type: 'error', title: message });
      }
    }
  }

  return (
    <Dialog open={open} onClose={onClose} title="تعطيل المستخدم">
      <div className="space-y-4">
        <div className="flex items-center gap-3 text-[var(--color-warning)]">
          <AlertTriangle size={24} />
          <p className="text-[var(--color-on-surface-variant)]">
            هل أنت متأكد من تعطيل حساب "{userName}"؟ سيتم إلغاء جميع جلساته النشطة فوراً.
          </p>
        </div>
        <div className="flex gap-2 justify-end">
          <Button variant="ghost" onClick={onClose}>إلغاء</Button>
          <Button variant="primary" onClick={handleDeactivate} loading={deactivate.isPending}>
            تعطيل
          </Button>
        </div>
      </div>
    </Dialog>
  );
}
