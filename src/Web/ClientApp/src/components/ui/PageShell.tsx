import type { ReactNode } from 'react';

interface PageShellProps {
  title?: string;
  subtitle?: string;
  header?: ReactNode;
  toolbar?: ReactNode;
  children: ReactNode;
}

export function PageShell({
  title,
  subtitle,
  header,
  toolbar,
  children,
}: PageShellProps) {
  return (
    <div className="flex flex-col gap-6 p-6 min-h-[calc(100vh-64px)]">
      {(title || header || toolbar) && (
        <div className="flex flex-col gap-2">
          {header ?? (
            <>
              {title && (
                <h1 className="text-headline-md font-bold text-[var(--color-on-surface)]">
                  {title}
                </h1>
              )}
              {subtitle && (
                <p className="text-body-md text-[var(--color-on-surface-variant)]">
                  {subtitle}
                </p>
              )}
            </>
          )}
          {toolbar && <div className="mt-2">{toolbar}</div>}
        </div>
      )}
      <div className="flex-1">{children}</div>
    </div>
  );
}
