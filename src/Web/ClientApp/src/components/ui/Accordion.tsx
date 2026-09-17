import { useState, useCallback, createContext, useContext, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { ChevronDown } from 'lucide-react';

interface AccordionItemProps {
  value: string;
  title: string;
  children: ReactNode;
  disabled?: boolean;
  defaultOpen?: boolean;
}

interface AccordionContextValue {
  openItems: Set<string>;
  toggle: (value: string) => void;
}

const AccordionContext = createContext<AccordionContextValue>({
  openItems: new Set(),
  toggle: () => {},
});

interface AccordionProps {
  children: ReactNode;
  type?: 'single' | 'multiple';
  defaultValue?: string[];
  className?: string;
}

function AccordionItem({
  value,
  title,
  children,
  disabled = false,
}: AccordionItemProps) {
  const { openItems, toggle } = useAccordionContext();
  const isOpen = openItems.has(value);

  return (
    <div
      className={cn(
        'border-b border-[var(--color-container-border)]',
        disabled && 'opacity-50 pointer-events-none'
      )}
    >
      <h3>
        <button
          type="button"
          onClick={() => toggle(value)}
          aria-expanded={isOpen}
          aria-disabled={disabled}
          className={cn(
            'flex items-center justify-between w-full py-4 px-2 text-start',
            'text-sm font-medium text-[var(--color-on-surface)]',
            'hover:bg-[var(--color-surface-container-low)] transition-colors',
            'focus-visible:outline-2 focus-visible:outline-secondary',
            'min-h-11'
          )}
        >
          <span>{title}</span>
          <ChevronDown
            size={16}
            aria-hidden="true"
            className={cn(
              'text-[var(--color-on-surface-variant)] transition-transform duration-200',
              isOpen && 'rotate-180'
            )}
          />
        </button>
      </h3>
      <div
        role="region"
        hidden={!isOpen}
        className="px-2 pb-4 text-sm text-[var(--color-on-surface-variant)]"
      >
        {children}
      </div>
    </div>
  );
}

function Accordion({
  children,
  type = 'single',
  defaultValue = [],
  className,
}: AccordionProps) {
  const [openItems, setOpenItems] = useState<Set<string>>(
    () => new Set(defaultValue)
  );

  const toggle = useCallback(
    (value: string) => {
      setOpenItems((prev) => {
        const next = new Set(prev);
        if (next.has(value)) {
          next.delete(value);
        } else {
          if (type === 'single') {
            next.clear();
          }
          next.add(value);
        }
        return next;
      });
    },
    [type]
  );

  return (
    <AccordionContext.Provider value={{ openItems, toggle }}>
      <div
        className={cn(
          'border border-[var(--color-container-border)] rounded-lg overflow-hidden bg-[var(--color-surface)]',
          className
        )}
        role="presentation"
      >
        {children}
      </div>
    </AccordionContext.Provider>
  );
}

function useAccordionContext() {
  const context = useContext(AccordionContext);
  if (!context) {
    throw new Error('Accordion components must be used within an Accordion');
  }
  return context;
}

export { Accordion, AccordionItem };
export type { AccordionProps, AccordionItemProps };
