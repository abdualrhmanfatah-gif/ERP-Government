import { notify } from '@/features/notifications/notify';
import { CheckCircle } from 'lucide-react';
import { ConfirmDialog } from '@/components/ui';
import { useReactivateUser } from '@/features/security/users/hooks';

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
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={handleReactivate}
      title="تنشيط المستخدم"
      confirmLabel="تنشيط"
      icon={<CheckCircle size={24} aria-hidden="true" />}
      message={`هل أنت متأكد من تنشيط حساب "${userName}"؟ سيتمكن من تسجيل الدخول مرة أخرى.`}
      loading={reactivate.isPending}
    />
  );
}
