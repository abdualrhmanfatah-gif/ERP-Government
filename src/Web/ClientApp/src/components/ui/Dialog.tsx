import { useEffect, useId, useRef, type ReactNode } from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

interface DialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
}

export function Dialog({ open, onClose, title, children, footer }: DialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const el = dialogRef.current;
    if (!el) return;
    if (open && !el.open) {
      el.showModal();
    } else if (!open && el.open) {
      el.close();
    }
  }, [open]);

  useEffect(() => {
    const el = dialogRef.current;
    if (!el) return;
    const handleClose = () => onClose();
    el.addEventListener('close', handleClose);
    return () => el.removeEventListener('close', handleClose);
  }, [onClose]);

  return (
    <dialog
      ref={dialogRef}
      aria-modal="true"
      aria-labelledby={titleId}
      className={cn(
        'backdrop:bg-black/50 m-auto p-0 w-full max-w-dialog rounded-lg border-0 shadow-transient bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)]'
      )}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--color-border-container)]">
        <h2
          id={titleId}
          className="text-base font-semibold leading-normal m-0"
        >
          {title}
        </h2>
        <button
          type="button"
          aria-label="إغلاق"
          onClick={onClose}
          className="inline-flex items-center justify-center w-11 h-11 border-none rounded-lg bg-transparent text-[var(--color-on-surface-variant)] cursor-pointer p-0 hover:text-[var(--color-on-surface)] hover:bg-[var(--color-surface-container-low)] transition-colors duration-150"
        >
          <X size={18} />
        </button>
      </div>

      {/* Body */}
      <div className="px-6 py-6">{children}</div>

      {/* Footer */}
      {footer ? (
        <div className="flex justify-end items-center gap-2 px-6 py-3 border-t border-[var(--color-border-container)]">
          {footer}
        </div>
      ) : null}
    </dialog>
  );
}
