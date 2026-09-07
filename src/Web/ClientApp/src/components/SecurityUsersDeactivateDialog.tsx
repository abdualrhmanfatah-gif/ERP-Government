import { notify } from '@/features/notifications/notify';
import { AlertTriangle } from 'lucide-react';
import { ConfirmDialog } from '@/components/ui';
import { useDeactivateUser } from '@/features/security/users/hooks';

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
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={handleDeactivate}
      title="تعطيل المستخدم"
      confirmLabel="تعطيل"
      destructive
      icon={<AlertTriangle size={24} aria-hidden="true" />}
      message={`هل أنت متأكد من تعطيل حساب "${userName}"؟ سيتم إلغاء جميع جلساته النشطة فوراً.`}
      loading={deactivate.isPending}
    />
  );
}
