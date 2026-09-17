import { useState, useRef, useEffect, useCallback, useId, type KeyboardEvent } from 'react';
import { Search, Check, ChevronDown, X } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ComboboxOption {
  value: string;
  label: string;
  disabled?: boolean;
}

interface ComboboxProps {
  options: ComboboxOption[];
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  label?: string;
  error?: string;
  disabled?: boolean;
  loading?: boolean;
  emptyMessage?: string;
  className?: string;
}

export function Combobox({
  options,
  value,
  onChange,
  placeholder = 'اختر...',
  searchPlaceholder = 'بحث...',
  label,
  error,
  disabled = false,
  loading = false,
  emptyMessage = 'لا توجد نتائج',
  className,
}: ComboboxProps) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const [activeIndex, setActiveIndex] = useState(-1);
  const [openUp, setOpenUp] = useState(false);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLUListElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((opt) => opt.value === value);

  const filtered = options.filter((opt) =>
    opt.label.toLowerCase().includes(query.toLowerCase())
  );

  const computeOpenUp = () => {
    const btn = buttonRef.current;
    if (!btn) return false;
    const rect = btn.getBoundingClientRect();
    const estHeight = 280;
    const spaceBelow = window.innerHeight - rect.bottom;
    const spaceAbove = rect.top;
    return spaceBelow < estHeight && spaceAbove > spaceBelow;
  };

  const openDropdown = () => {
    setOpenUp(computeOpenUp());
    setOpen(true);
    inputRef.current?.focus();
  };

  const handleSelect = useCallback(
    (val: string) => {
      onChange?.(val === value ? '' : val);
      setOpen(false);
      setQuery('');
      setActiveIndex(-1);
      inputRef.current?.focus();
    },
    [onChange, value]
  );

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (disabled) return;

      switch (e.key) {
        case 'Enter':
          e.preventDefault();
          if (open && activeIndex >= 0 && filtered[activeIndex]) {
            handleSelect(filtered[activeIndex].value);
          } else if (!open) {
            openDropdown();
          }
          break;
        case 'ArrowDown':
          e.preventDefault();
          if (!open) {
            openDropdown();
          } else {
            setActiveIndex((prev) =>
              prev < filtered.length - 1 ? prev + 1 : 0
            );
          }
          break;
        case 'ArrowUp':
          e.preventDefault();
          if (open) {
            setActiveIndex((prev) =>
              prev > 0 ? prev - 1 : filtered.length - 1
            );
          }
          break;
        case 'Escape':
          setOpen(false);
          setQuery('');
          setActiveIndex(-1);
          break;
        case 'Backspace':
          if (!query && value) {
            onChange?.('');
          }
          break;
      }
    },
    [disabled, open, activeIndex, filtered, handleSelect, query, value, onChange]
  );

  useEffect(() => {
    if (open && activeIndex >= 0 && listRef.current) {
      const item = listRef.current.children[activeIndex] as HTMLElement;
      item?.scrollIntoView({ block: 'nearest' });
    }
  }, [activeIndex, open]);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
        setQuery('');
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const autoId = useId();
  const inputId = autoId;
  const errorId = `${inputId}-error`;

  return (
    <div className={cn('flex flex-col gap-1', className)} ref={containerRef}>
      {label && (
        <label htmlFor={inputId} className="text-label-md text-[var(--color-on-surface)]">
          {label}
        </label>
      )}
      <div className="relative">
        <button
          type="button"
          ref={buttonRef}
          id={inputId}
          role="combobox"
          aria-expanded={open}
          aria-haspopup="listbox"
          aria-invalid={!!error || undefined}
          aria-describedby={error ? errorId : undefined}
          disabled={disabled}
          onKeyDown={handleKeyDown}
          onClick={() => {
            if (!disabled) {
              if (open) {
                setOpen(false);
              } else {
                openDropdown();
              }
            }
          }}
          className={cn(
            'flex items-center justify-between w-full h-[var(--density-comfortable-control-height)] px-3 py-2 text-sm rounded-lg border bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)] cursor-pointer transition-colors duration-150',
            'focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:shadow-[0_0_0_4px_var(--color-focus-halo)]',
            'disabled:bg-[var(--color-disabled-bg)] disabled:text-[var(--color-disabled-fg)] disabled:cursor-not-allowed',
            error
              ? 'border-[var(--color-error)] focus:outline-[var(--color-error)] focus:shadow-[0_0_0_4px_var(--color-error-container)]'
              : 'border-[var(--color-input-border)]'
          )}
        >
          <span className={cn(!selectedOption && 'text-[var(--color-outline)]')}>
            {selectedOption ? selectedOption.label : placeholder}
          </span>
          <ChevronDown
            size={16}
            className={cn(
              'text-[var(--color-on-surface-variant)] transition-transform duration-150',
              open && 'rotate-180'
            )}
            aria-hidden="true"
          />
        </button>

        {open && (
          <div className={`absolute z-[300] w-full ${openUp ? 'bottom-full mb-1' : 'top-full mt-1'} bg-[var(--color-surface-container-lowest)] rounded-lg border border-[var(--color-container-border)] shadow-lg overflow-hidden`}>
            {/* Search input */}
            <div className="flex items-center gap-2 px-3 py-2 border-b border-[var(--color-container-border)]">
              <Search size={16} className="text-[var(--color-on-surface-variant)] shrink-0" aria-hidden="true" />
              <input
                ref={inputRef}
                type="text"
                value={query}
                onChange={(e) => {
                  setQuery(e.target.value);
                  setActiveIndex(-1);
                }}
                onKeyDown={handleKeyDown}
                placeholder={searchPlaceholder}
                className="flex-1 bg-transparent border-none outline-none text-sm text-[var(--color-on-surface)] placeholder:text-[var(--color-outline)]"
                aria-label={searchPlaceholder}
              />
              {query && (
                <button
                  type="button"
                  onClick={() => setQuery('')}
                  className="text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)] p-2 min-w-9 min-h-9 border-none bg-transparent cursor-pointer rounded-lg hover:bg-[var(--color-surface-container-low)]"
                  aria-label="مسح البحث"
                >
                  <X size={16} />
                </button>
              )}
            </div>

            {/* Options list */}
            <ul
              ref={listRef}
              role="listbox"
              aria-label={label || 'الخيارات'}
              className="max-h-60 overflow-auto p-1 m-0 list-none"
            >
              {loading ? (
                <li className="px-3 py-2 text-sm text-[var(--color-on-surface-variant)] text-center">
                  جاري التحميل...
                </li>
              ) : filtered.length === 0 ? (
                <li className="px-3 py-2 text-sm text-[var(--color-on-surface-variant)] text-center">
                  {emptyMessage}
                </li>
              ) : (
                filtered.map((opt, index) => (
                  <li
                    key={opt.value}
                    role="option"
                    aria-selected={opt.value === value}
                    aria-disabled={opt.disabled}
                    onClick={() => {
                      if (!opt.disabled) handleSelect(opt.value);
                    }}
                    onMouseEnter={() => setActiveIndex(index)}
                    className={cn(
                      'flex items-center justify-between px-3 py-2 text-sm rounded-lg cursor-pointer transition-colors duration-100',
                      opt.disabled && 'opacity-50 cursor-not-allowed',
                      opt.value === value
                        ? 'bg-[var(--color-surface-container-low)] text-[var(--color-primary)] font-medium'
                        : activeIndex === index
                          ? 'bg-[var(--color-surface-container-low)]'
                          : 'text-[var(--color-on-surface)] hover:bg-[var(--color-surface-container-low)]'
                    )}
                  >
                    <span>{opt.label}</span>
                    {opt.value === value && (
                      <Check size={16} className="text-[var(--color-primary)] shrink-0" aria-hidden="true" />
                    )}
                  </li>
                ))
              )}
            </ul>
          </div>
        )}
      </div>
      {error && (
        <p id={errorId} role="alert" className="text-xs text-[var(--color-error)]">
          {error}
        </p>
      )}
    </div>
  );
}
