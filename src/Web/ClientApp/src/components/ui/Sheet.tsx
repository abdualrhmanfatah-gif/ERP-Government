import { useEffect, useId, useRef, type KeyboardEvent, type ReactNode } from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

type SheetSide = 'right' | 'left' | 'top' | 'bottom';

interface SheetProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  side?: SheetSide;
  footer?: ReactNode;
  className?: string;
}

const sideClasses: Record<SheetSide, string> = {
  right: 'inset-y-0 end-0 border-e',
  left: 'inset-y-0 start-0 border-s',
  top: 'inset-x-0 top-0 border-t',
  bottom: 'inset-x-0 bottom-0 border-b',
};

const slideClasses: Record<SheetSide, string> = {
  right: 'translate-x-full',
  left: '-translate-x-full',
  top: '-translate-y-full',
  bottom: 'translate-y-full',
};

export function Sheet({
  open,
  onClose,
  title,
  children,
  side = 'right',
  footer,
  className,
}: SheetProps) {
  const sheetRef = useRef<HTMLDivElement>(null);
  const previouslyFocused = useRef<HTMLElement | null>(null);
  const titleId = useId();

  useEffect(() => {
    function handleEscape(e: globalThis.KeyboardEvent) {
      if (e.key === 'Escape' && open) {
        onClose();
      }
    }
    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [open, onClose]);

  useEffect(() => {
    if (!open) return;
    previouslyFocused.current = document.activeElement as HTMLElement | null;
    sheetRef.current?.focus();
    return () => {
      previouslyFocused.current?.focus?.();
    };
  }, [open]);

  function handleKeyDown(e: KeyboardEvent<HTMLDivElement>) {
    if (e.key !== 'Tab') return;
    const panel = sheetRef.current;
    if (!panel) return;
    const focusables = panel.querySelectorAll<HTMLElement>(
      'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
    );
    if (focusables.length === 0) return;
    const first = focusables[0];
    const last = focusables[focusables.length - 1];
    if (e.shiftKey && document.activeElement === first) {
      e.preventDefault();
      last.focus();
    } else if (!e.shiftKey && document.activeElement === last) {
      e.preventDefault();
      first.focus();
    }
  }

  useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-[1040]">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-[var(--color-overlay)] backdrop-blur-sm animate-in fade-in duration-200"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Sheet panel */}
      <div
        ref={sheetRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
        onKeyDown={handleKeyDown}
        className={cn(
          'absolute flex flex-col bg-[var(--color-surface-container-lowest)] shadow-xl',
          'transition-transform duration-300 ease-out',
          sideClasses[side],
          side === 'right' || side === 'left'
            ? 'w-full max-w-md h-full'
            : 'h-full max-h-[80vh] w-full',
          open ? 'translate-x-0 translate-y-0' : slideClasses[side],
          className
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--color-container-border)] shrink-0">
          <h2
            id={titleId}
            className="text-base font-semibold leading-normal m-0 text-[var(--color-on-surface)]"
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
        <div className="flex-1 overflow-auto px-6 py-6">{children}</div>

        {/* Footer */}
        {footer ? (
          <div className="flex justify-end items-center gap-2 px-6 py-3 border-t border-[var(--color-container-border)] shrink-0">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  );
}
