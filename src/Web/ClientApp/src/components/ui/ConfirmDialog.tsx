import type { ReactNode } from 'react';
import { Dialog } from './Dialog';
import { Button } from './Button';

interface ConfirmDialogProps {
  open?: boolean;
  onClose: () => void;
  onConfirm: () => void;
  message: ReactNode;
  title?: string;
  confirmLabel?: string;
  destructive?: boolean;
  loading?: boolean;
  icon?: ReactNode;
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
  icon,
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
      {icon ? (
        <div className="flex items-start gap-3">
          <span className={destructive ? 'text-[var(--color-error)]' : 'text-[var(--color-primary)]'} aria-hidden="true">
            {icon}
          </span>
          <div className="text-sm leading-relaxed text-[var(--color-on-surface-variant)]">{message}</div>
        </div>
      ) : (
        <div className="text-sm leading-relaxed text-[var(--color-on-surface-variant)]">{message}</div>
      )}
    </Dialog>
  );
}
