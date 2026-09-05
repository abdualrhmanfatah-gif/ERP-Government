import { useState, useRef, type ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface Tab {
  key: string;
  label: string;
  content: ReactNode;
  icon?: ReactNode;
}

interface TabsProps {
  tabs: Tab[];
  defaultKey?: string;
  onChange?: (key: string) => void;
}

export function Tabs({ tabs, defaultKey, onChange }: TabsProps) {
  const [activeKey, setActiveKey] = useState(defaultKey || tabs[0]?.key || '');
  const tabRefs = useRef<Map<string, HTMLButtonElement>>(new Map());

  const handleChange = (key: string) => {
    setActiveKey(key);
    onChange?.(key);
  };

  const handleKeyDown = (e: React.KeyboardEvent, currentKey: string) => {
    const keys = tabs.map((t) => t.key);
    const idx = keys.indexOf(currentKey);
    let nextIdx: number | null = null;
    const isRtl = document.documentElement.dir === 'rtl';

    if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
      nextIdx = isRtl
        ? (idx - 1 + keys.length) % keys.length
        : (idx + 1) % keys.length;
    } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
      nextIdx = isRtl
        ? (idx + 1) % keys.length
        : (idx - 1 + keys.length) % keys.length;
    } else if (e.key === 'Home') {
      nextIdx = 0;
    } else if (e.key === 'End') {
      nextIdx = keys.length - 1;
    }

    if (nextIdx !== null) {
      e.preventDefault();
      const nextKey = keys[nextIdx];
      tabRefs.current.get(nextKey)?.focus();
      handleChange(nextKey);
    }
  };

  const activeTab = tabs.find((t) => t.key === activeKey);

  return (
    <div className="flex flex-col">
      <div
        role="tablist"
        className="flex border-b border-[var(--color-border-container)]"
      >
        {tabs.map((tab) => {
          const isActive = tab.key === activeKey;
          return (
            <button
              key={tab.key}
              ref={(el) => {
                if (el) tabRefs.current.set(tab.key, el);
              }}
              type="button"
              role="tab"
              id={`tab-${tab.key}`}
              aria-selected={isActive}
              aria-controls={`tabpanel-${tab.key}`}
              tabIndex={isActive ? 0 : -1}
              onClick={() => handleChange(tab.key)}
              onKeyDown={(e) => handleKeyDown(e, tab.key)}
              className={cn(
                'flex items-center gap-2 px-4 py-3 min-h-11 text-sm font-medium border-b-2 transition-colors duration-150',
                isActive
                  ? 'border-[var(--color-primary)] text-[var(--color-on-surface)]'
                  : 'border-transparent text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)]'
              )}
            >
              {tab.icon && <span aria-hidden="true">{tab.icon}</span>}
              {tab.label}
            </button>
          );
        })}
      </div>
      <div
        role="tabpanel"
        id={`tabpanel-${activeKey}`}
        aria-labelledby={`tab-${activeKey}`}
        className="py-4"
      >
        {activeTab?.content}
      </div>
    </div>
  );
}
