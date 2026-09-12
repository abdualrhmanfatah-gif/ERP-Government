import { useState, useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../shared/hooks/useAuth';
import { useUserProfile } from '../shared/hooks/useUserProfile';
import { NotificationBell } from '../features/notifications';
import { ThemeToggle } from '../components/ThemeToggle';
import { Menu, X, User } from 'lucide-react';
import { Sidebar } from './Sidebar';
import { TopNav } from './TopNav';
import { Button } from '@/components/ui';

export function AppLayout({ children }: { children: React.ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const profileRef = useRef<HTMLDivElement>(null);
  const { isAuthenticated, logout } = useAuth();
  const userProfile = useUserProfile();
  const location = useLocation();
  const prevPathRef = useRef(location.pathname);

  useEffect(() => {
    if (prevPathRef.current !== location.pathname) {
      prevPathRef.current = location.pathname;
      setSidebarOpen(false);
    }
  }, [location.pathname]);

  // Lock body scroll when drawer open (mobile) + Escape handling
  useEffect(() => {
    if (sidebarOpen) {
      const prev = document.body.style.overflow;
      document.body.style.overflow = 'hidden';
      return () => { document.body.style.overflow = prev; };
    }
  }, [sidebarOpen]);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) {
        setProfileOpen(false);
      }
    }
    function handleEscape(e: KeyboardEvent) {
      if (e.key === 'Escape') {
        setProfileOpen(false);
        setSidebarOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleEscape);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleEscape);
    };
  }, []);


  return (
    <div className="flex min-h-screen bg-[var(--color-surface-container-low)]">
      {/* Skip navigation — WCAG 2.4.1 */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:top-2 focus:start-2 focus:z-[9999] focus:px-4 focus:py-2 focus:bg-[var(--color-primary)] focus:text-[var(--color-on-primary)] focus:rounded-lg focus:shadow-lg focus:outline-2 focus:outline-[var(--color-focus-ring)] focus:outline-offset-2 focus:shadow-[0_0_0_4px_var(--color-focus-halo)]"
      >
        تخطي إلى المحتوى الرئيسي
      </a>

      <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      {/* Main area */}
      <div className="flex-1 flex flex-col min-h-screen min-w-0">
        {/* Header — 64px sticky, z-30 per scale */}
        <header className="h-16 sticky top-0 z-30 bg-[var(--color-primary)] border-b border-white/10 flex items-center px-4 sm:px-6 gap-3 sm:gap-4 shadow-sm">
          {/* Mobile menu toggle */}
          <Button
            variant="header"
            size="icon"
            className="lg:hidden"
            aria-label={sidebarOpen ? 'إغلاق القائمة' : 'فتح القائمة'}
            aria-expanded={sidebarOpen}
            onClick={() => setSidebarOpen(!sidebarOpen)}
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </Button>

          {/* Brand — desktop only; drawer owns its brand on mobile */}
          <Link
            to="/"
            className="hidden lg:inline-flex items-center gap-2 text-headline-md font-bold text-white no-underline shrink-0 cursor-pointer focus-visible:ring-2 focus-visible:ring-white/50 rounded-lg px-1 -ms-1"
          >
            <span className="w-1 h-5 rounded-full bg-[var(--color-secondary)]" aria-hidden="true" />
            ERP Government
          </Link>

          {/* Top navigation — desktop dropdowns */}
          <TopNav />

          <div className="flex-1" aria-hidden="true" />

          {/* Right side actions */}
          <div className="flex items-center gap-1 sm:gap-2 shrink-0">
            {/* Notification bell */}
            <NotificationBell />

            <ThemeToggle />

            {/* User profile */}
            {isAuthenticated ? (
              <div ref={profileRef} className="relative">
                <Button
                  variant="header"
                  className="flex items-center gap-2"
                  aria-label="حساب المستخدم"
                  aria-expanded={profileOpen}
                  onClick={() => { setProfileOpen(!profileOpen); }}
                >
                  <User size={18} />
                  <span className="text-body-sm text-white/90 hidden sm:inline max-w-[12ch] truncate">{userProfile.name}</span>
                </Button>
                {profileOpen ? (
                  <div
                    role="menu"
                    aria-label="حساب المستخدم"
                    className="absolute end-0 sm:start-0 top-full mt-2 w-56 bg-[var(--color-surface)] rounded-xl shadow-xl border border-[var(--color-border-container)] z-50 overflow-hidden"
                  >
                    <div className="px-4 py-3 border-b border-[var(--color-border-container)]">
                      <div className="text-body-sm font-semibold text-[var(--color-on-surface)]">{userProfile.name}</div>
                      {userProfile.role ? (
                        <div className="text-label-sm text-[var(--color-on-surface-variant)] mt-0.5">{userProfile.role}</div>
                      ) : null}
                    </div>
                    <Button
                      variant="ghost"
                      role="menuitem"
                      className="w-full text-start px-4 py-3 text-body-sm text-[var(--color-error)] hover:bg-[var(--color-surface-container-high)] transition-colors duration-200 cursor-pointer"
                      onClick={() => { setProfileOpen(false); logout(); }}
                    >
                      تسجيل الخروج
                    </Button>
                  </div>
                ) : null}
              </div>
            ) : null}
          </div>
        </header>

        {/* Page content */}
        <main id="main-content" className="flex-1 px-4 sm:px-6 lg:px-8 py-0.5 sm:py-1 bg-[var(--color-surface-container-low)] min-w-0">
          <div className="max-w-[1440px] mx-auto w-full">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
