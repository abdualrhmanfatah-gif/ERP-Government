import { useState, useEffect, useCallback } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { moduleGroups } from './navigation';
import { Button } from '@/components/ui';

const STORAGE_KEY = 'erpSidebarCollapsed';

function getStoredCollapsed(): Record<string, boolean> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function storeCollapsed(state: Record<string, boolean>) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  } catch {
    // localStorage unavailable — silently ignore
  }
}

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export function Sidebar({ open, onClose }: SidebarProps) {
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>(getStoredCollapsed);
  const location = useLocation();

  useEffect(() => {
    storeCollapsed(collapsed);
  }, [collapsed]);

  const toggleGroup = useCallback((label: string) => {
    setCollapsed((prev) => ({ ...prev, [label]: !prev[label] }));
  }, []);

  const isGroupExpanded = (label: string) => !collapsed[label];

  return (
    <>
      {/* Mobile backdrop — z-40 per scale */}
      {open ? (
        <div
          className="fixed inset-0 bg-slate-950/50 backdrop-blur-sm z-40 lg:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      ) : null}

      {/* Sidebar — drawer anchored to end edge for RTL (logical end-0), z-50 */}
      <aside
        aria-label="القائمة الرئيسية"
        style={{ background: 'var(--gradient-sidebar, #002045)' }}
        className={`w-[300px] max-w-[85vw] text-white flex flex-col fixed inset-y-0 end-0 z-50 lg:hidden shadow-2xl transition-transform duration-300 ease-out will-change-transform ${
          open ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        {/* Brand */}
        <div className="h-16 flex items-center justify-between px-6 border-b border-white/10 shrink-0">
          <Link to="/" onClick={onClose} className="text-white no-underline text-headline-md font-bold min-h-11 inline-flex items-center gap-2 cursor-pointer">
            <span className="w-1 h-5 rounded-full bg-[var(--color-secondary)] shrink-0" aria-hidden="true" />
            ERP Government
          </Link>
          <Button
            variant="header"
            size="icon-xs"
            onClick={onClose}
            aria-label="إغلاق القائمة"
          >
            <ChevronRight size={20} className="rotate-180 lg:rotate-0" aria-hidden="true" />
          </Button>
        </div>

        {/* Module groups */}
        <nav className="flex-1 overflow-y-auto py-3">
          <ul className="list-none m-0 p-0" role="tree">
            {moduleGroups.map((group) => {
              const expanded = isGroupExpanded(group.label);
              return (
                <li key={group.label} role="treeitem" aria-expanded={expanded}>
                  <Button
                    variant="header"
                    className="w-full flex items-center justify-between px-6 py-3 min-h-11 text-body-md font-semibold uppercase"
                    onClick={() => toggleGroup(group.label)}
                    aria-label={` ${group.label}`}
                  >
                    <span role="heading" aria-level={2}>{group.label}</span>
                    {expanded ? (
                      <ChevronDown size={14} aria-hidden="true" />
                    ) : (
                      <ChevronRight size={14} aria-hidden="true" />
                    )}
                  </Button>
                  {expanded ? (
                    <ul className="list-none m-0 p-0" role="group">
                      {group.items.map((item) => {
                        const active = location.pathname.startsWith(item.path);
                        return (
                          <li key={item.path} role="treeitem">
                            <Link
                              to={item.path}
                              aria-current={active ? 'page' : undefined}
                              className={`flex items-center gap-3 px-6 py-3 min-h-11 pe-10 no-underline text-body-md transition-colors duration-150 cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/40 focus-visible:ring-inset ${
                                active
                                  ? 'text-white bg-white/10 font-semibold border-e-[3px] border-e-[var(--color-secondary)]'
                                  : 'text-white/70 border-e-[3px] border-e-transparent hover:text-white hover:bg-white/[0.06]'
                              }`}
                            >
                              {item.label}
                            </Link>
                          </li>
                        );
                      })}
                    </ul>
                  ) : null}
                </li>
              );
            })}
          </ul>
        </nav>
      </aside>
    </>
  );
}
