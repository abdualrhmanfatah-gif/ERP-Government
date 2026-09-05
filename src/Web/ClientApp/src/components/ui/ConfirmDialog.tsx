import { Dialog } from './Dialog';
import { Button } from './Button';

interface ConfirmDialogProps {
  open?: boolean;
  onClose: () => void;
  onConfirm: () => void;
  message: string;
  title?: string;
  confirmLabel?: string;
  destructive?: boolean;
  loading?: boolean;
}

export function ConfirmDialog({
  open = false,
  onClose,
  onConfirm,
  message,
  title = 'تأكيد',
  confirmLabel = 'تأكيد',
  destructive = false,
  loading = false,
}: ConfirmDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="ghost" onClick={onClose} disabled={loading}>
            إلغاء
          </Button>
          <Button
            variant={destructive ? 'destructive' : 'primary'}
            onClick={onConfirm}
            disabled={loading}
            aria-busy={loading || undefined}
          >
            {confirmLabel}
          </Button>
        </>
      }
    >
      <p className="m-0 text-sm leading-relaxed text-[var(--color-on-surface-variant)]">{message}</p>
    </Dialog>
  );
}
