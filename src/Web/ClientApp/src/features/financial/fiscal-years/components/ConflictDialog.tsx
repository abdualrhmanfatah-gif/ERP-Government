import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';

interface ConflictDialogProps {
  open: boolean;
  onClose: () => void;
  onRefresh: () => void;
  message?: string;
}

export function ConflictDialog({
  open,
  onClose,
  onRefresh,
  message = 'تم تعديل هذا السجل بواسطة مستخدم آخر. يرجى تحديث البيانات والمحاولة مرة أخرى.',
}: ConflictDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="تعارض في البيانات"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>
            إلغاء
          </Button>
          <Button variant="primary" onClick={onRefresh}>
            تحديث البيانات
          </Button>
        </>
      }
    >
      <p className="m-0 text-sm leading-relaxed text-[var(--color-outline)]">{message}</p>
    </Dialog>
  );
}
