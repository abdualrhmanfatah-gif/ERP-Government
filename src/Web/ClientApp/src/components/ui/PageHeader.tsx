import { useId } from 'react';
import type { ReactNode } from 'react';

interface PageHeaderProps {
  title: string;
  description?: string;
  actions?: ReactNode;
  className?: string;
}

export function PageHeader({ title, description, actions, className = '' }: PageHeaderProps) {
 
  const uniqueId = useId();
  const titleId = `page-title-${uniqueId}`;

  return (
    
    <header 
      className={`flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 border-b border-[var(--color-border-container)] pb-4 ${className}`}
    >
      <div className="border-s-[3px] border-s-[var(--color-secondary)] ps-3">
        <h1 
          id={titleId} 
          className="text-2xl font-bold leading-tight text-[var(--color-on-surface)]"
        >
          {title}
        </h1>
        {description && (
          <p className="mt-1.5 text-sm text-[var(--color-on-surface-variant)]">
            {description}
          </p>
        )}
      </div>
      
     
      {actions && (
        <div className="flex items-center gap-2 self-start sm:self-auto w-full sm:w-auto">
          {actions}
        </div>
      )}
    </header>
  );
}