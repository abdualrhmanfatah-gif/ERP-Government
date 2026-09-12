import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Breadcrumb, type BreadcrumbItem } from './Breadcrumb';
import { Loading } from './Loading';
import { ErrorState } from './ErrorState';

const maxWidthClasses = {
  sm: 'max-w-2xl mx-auto',
  md: 'max-w-4xl mx-auto',
  lg: 'max-w-6xl mx-auto',
  xl: 'max-w-7xl mx-auto',
  full: 'w-full',
} as const;

interface PageProps {
  title: string;
  description?: ReactNode;
  actions?: ReactNode;
  toolbar?: ReactNode;
  breadcrumbs?: BreadcrumbItem[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
  onBack?: () => void; // إضافة خاصية زر الرجوع
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | 'full';
  className?: string;
  children: ReactNode;
}

export function Page({
  title,
  description,
  actions,
  toolbar,
  breadcrumbs,
  loading,
  error,
  onRetry,
  onBack,
  maxWidth = 'full',
  className,
  children,
}: PageProps) {
  return (
    <div className={cn('flex flex-col gap-6 p-6 min-h-[calc(100vh-64px)]', maxWidthClasses[maxWidth], className)}>
      
      {/* مسار التنقل (Breadcrumbs) */}
      {breadcrumbs && breadcrumbs.length > 0 && (
        <Breadcrumb items={breadcrumbs} />
      )}

      {/* رأس الصفحة (Header) */}
      <header className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
        <div className="flex flex-col gap-1.5">
          {/* حاوية زر الرجوع والعنوان معاً على نفس السطر */}
          <div className="flex items-center gap-3">
            {onBack && (
              <button
                onClick={onBack}
                className="flex items-center justify-center w-8 h-8 rounded-full hover:bg-black/5 dark:hover:bg-white/10 text-[var(--color-on-surface)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
                aria-label="رجوع"
                type="button"
              >
                {/* أيقونة سهم (تتجه لليمين لتناسب الواجهات العربية RTL) */}
                <svg 
                  xmlns="http://www.w3.org/2000/svg" 
                  width="20" 
                  height="20" 
                  viewBox="0 0 24 24" 
                  fill="none" 
                  stroke="currentColor" 
                  strokeWidth="2.5" 
                  strokeLinecap="round" 
                  strokeLinejoin="round"
                >
                  <path d="M5 12h14M12 5l7 7-7 7" />
                </svg>
              </button>
            )}
            
            <h1 className="text-headline-md font-bold text-[var(--color-on-surface)] leading-none">
              {title}
            </h1>
          </div>

          {/* الوصف (يظهر أسفل العنوان والسهم) */}
          {description && (
            <div className="text-sm text-[var(--color-on-surface-variant)] mt-1">
              {description}
            </div>
          )}
        </div>

        {/* الأزرار الجانبية (Actions) */}
        {actions && (
          <div className="flex items-center gap-2 mt-2 sm:mt-0">
            {actions}
          </div>
        )}
      </header>

      {/* شريط الأدوات (Toolbar) */}
      {toolbar && <div>{toolbar}</div>}

      {/* محتوى الصفحة (Main Content) */}
      <main className="flex-1">
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <Loading />
          </div>
        ) : error ? (
          <ErrorState message={error} onRetry={onRetry} />
        ) : (
          children
        )}
      </main>
    </div>
  );
}