import { useState, useRef, useEffect, useCallback } from 'react';
import { useIso4217Codes } from '../../hooks/useCurrencies';
import type { Iso4217CodeDto } from '../../shared/types';

interface Iso4217PickerProps {
  onSelect: (code: Iso4217CodeDto) => void;
  selectedCode?: Iso4217CodeDto | null;
  disabled?: boolean;
}

export function Iso4217Picker({ onSelect, selectedCode, disabled }: Iso4217PickerProps) {
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLUListElement>(null);
  const { data: codes = [], isLoading } = useIso4217Codes(query || undefined);

  const handleSelect = useCallback((code: Iso4217CodeDto) => {
    onSelect(code);
    setQuery(code.code);
    setIsOpen(false);
    setActiveIndex(-1);
  }, [onSelect]);

  useEffect(() => {
    if (!isOpen) return;
    const list = listRef.current;
    if (!list) return;
    const items = list.querySelectorAll('[role="option"]');
    if (activeIndex >= 0 && items[activeIndex]) {
      (items[activeIndex] as HTMLElement).scrollIntoView({ block: 'nearest' });
    }
  }, [activeIndex, isOpen]);

  function handleKeyDown(e: React.KeyboardEvent) {
    if (!isOpen) {
      if (e.key === 'ArrowDown' || e.key === 'Enter') {
        setIsOpen(true);
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setActiveIndex((i) => (i < codes.length - 1 ? i + 1 : 0));
        break;
      case 'ArrowUp':
        e.preventDefault();
        setActiveIndex((i) => (i > 0 ? i - 1 : codes.length - 1));
        break;
      case 'Enter':
        e.preventDefault();
        if (activeIndex >= 0 && codes[activeIndex]) {
          handleSelect(codes[activeIndex]);
        }
        break;
      case 'Escape':
        setIsOpen(false);
        setActiveIndex(-1);
        break;
    }
  }

  return (
    <div className="relative">
      <label htmlFor="isoSearch" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">
        بحث ISO 4217 *
      </label>
      <input
        ref={inputRef}
        id="isoSearch"
        type="text"
        value={query}
        onChange={(e) => {
          setQuery(e.target.value);
          setIsOpen(true);
          setActiveIndex(-1);
        }}
        onFocus={() => setIsOpen(true)}
        onBlur={() => setTimeout(() => setIsOpen(false), 150)}
        onKeyDown={handleKeyDown}
        placeholder="اكتب كود العملة مثل YER..."
        disabled={disabled}
        className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:border-[var(--color-focus-ring)]"
        role="combobox"
        aria-expanded={isOpen}
        aria-autocomplete="list"
        aria-controls="iso4217-listbox"
      />
      {isOpen && codes.length > 0 && (
        <ul
          ref={listRef}
          id="iso4217-listbox"
          role="listbox"
          className="absolute z-[var(--z-index-dropdown)] mt-1 w-full max-h-40 overflow-y-auto border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface)] shadow-[var(--shadow-sm)]"
        >
          {codes.map((code, index) => (
            <li
              key={code.code}
              role="option"
              aria-selected={selectedCode?.code === code.code}
              onMouseDown={() => handleSelect(code)}
              onMouseEnter={() => setActiveIndex(index)}
              className={`w-full text-start px-3 py-2 text-sm cursor-pointer transition-colors ${
                activeIndex === index
                  ? 'bg-[var(--color-primary-container)]'
                  : selectedCode?.code === code.code
                    ? 'bg-[var(--color-primary-container)]'
                    : 'hover:bg-[var(--color-surface-container)]'
              }`}
            >
              <span className="font-mono">{code.code}</span> — {code.name}
            </li>
          ))}
        </ul>
      )}
      {isOpen && query && codes.length === 0 && !isLoading && (
        <div className="absolute z-[var(--z-index-dropdown)] mt-1 w-full px-3 py-2 text-sm text-[var(--color-on-surface-variant)] bg-[var(--color-surface)] border border-[var(--color-border-container)] rounded-lg">
          لا توجد نتائج
        </div>
      )}
    </div>
  );
}
