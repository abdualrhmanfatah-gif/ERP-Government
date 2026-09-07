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
  full: '',
} as const;

interface PageProps {
  title: string;
  description?: string;
  actions?: ReactNode;
  toolbar?: ReactNode;
  breadcrumbs?: BreadcrumbItem[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
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
  maxWidth = 'full',
  className,
  children,
}: PageProps) {
  return (
    <div dir="rtl" className={cn('flex flex-col gap-6 p-6 min-h-[calc(100vh-64px)]', className)}>
      {breadcrumbs && breadcrumbs.length > 0 && (
        <Breadcrumb items={breadcrumbs} />
      )}

      <header className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-headline-md font-bold text-[var(--color-on-surface)]">
            {title}
          </h1>
          {description && (
            <p className="mt-1.5 text-sm text-[var(--color-on-surface-variant)]">
              {description}
            </p>
          )}
        </div>
        {actions && (
          <div className="flex items-center gap-2 self-start sm:self-auto">
            {actions}
          </div>
        )}
      </header>

      {toolbar && <div>{toolbar}</div>}

      <div className={cn('flex-1', maxWidthClasses[maxWidth])}>
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <Loading />
          </div>
        ) : error ? (
          <ErrorState message={error} onRetry={onRetry} />
        ) : (
          children
        )}
      </div>
    </div>
  );
}
