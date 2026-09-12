import { notify } from '@/features/notifications/notify';
import { AlertTriangle, CheckCircle } from 'lucide-react';
import { ConfirmDialog } from '@/components/ui';
import { useDeactivateUser, useReactivateUser } from '@/features/security/users/hooks';

interface UserStatusDialogProps {
  open: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
  action: 'deactivate' | 'reactivate';
}

const actionConfig = {
  deactivate: {
    title: 'تعطيل المستخدم',
    confirmLabel: 'تعطيل',
    destructive: true,
    icon: <AlertTriangle size={24} aria-hidden="true" />,
    message: (name: string) => `هل أنت متأكد من تعطيل حساب "${name}"؟ سيتم إلغاء جميع جلساته النشطة فوراً.`,
    successTitle: 'تم تعطيل المستخدم بنجاح',
    errorCheck: (msg: string) =>
      msg.includes('self') || msg.includes('own account')
        ? 'لا يمكن تعطيل حسابك الخاص'
        : msg,
  },
  reactivate: {
    title: 'تنشيط المستخدم',
    confirmLabel: 'تنشيط',
    destructive: false,
    icon: <CheckCircle size={24} aria-hidden="true" />,
    message: (name: string) => `هل أنت متأكد من تنشيط حساب "${name}"؟ سيتمكن من تسجيل الدخول مرة أخرى.`,
    successTitle: 'تم تنشيط المستخدم بنجاح',
    errorCheck: (msg: string) => msg,
  },
} as const;

export function UserStatusDialog({ open, onClose, userId, userName, action }: UserStatusDialogProps) {
  const deactivate = useDeactivateUser();
  const reactivate = useReactivateUser();
  const config = actionConfig[action];
  const mutation = action === 'deactivate' ? deactivate : reactivate;

  async function handleConfirm() {
    try {
      await mutation.mutateAsync(userId);
      notify({ type: 'success', title: config.successTitle });
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ';
      notify({ type: 'error', title: config.errorCheck(message) });
    }
  }

  return (
    <ConfirmDialog
      open={open}
      onClose={onClose}
      onConfirm={handleConfirm}
      title={config.title}
      confirmLabel={config.confirmLabel}
      destructive={config.destructive}
      icon={config.icon}
      message={config.message(userName)}
      loading={mutation.isPending}
    />
  );
}
