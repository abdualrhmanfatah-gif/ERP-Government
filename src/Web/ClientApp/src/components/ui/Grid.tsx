import type { ReactNode, CSSProperties } from 'react';
import { cn } from '@/lib/utils';

type GridColumns = 1 | 2 | 3 | 4 | 6 | 12;
type GridGap = 'xs' | 'sm' | 'md' | 'lg';

interface GridProps {
  columns?: GridColumns;
  gap?: GridGap;
  align?: CSSProperties['alignItems'];
  className?: string;
  children: ReactNode;
}

const columnsMap: Record<GridColumns, string> = {
  1: 'grid-cols-1',
  2: 'grid-cols-2',
  3: 'grid-cols-3',
  4: 'grid-cols-4',
  6: 'grid-cols-6',
  12: 'grid-cols-12',
};

const gapMap: Record<GridGap, string> = {
  xs: 'gap-1',
  sm: 'gap-2',
  md: 'gap-4',
  lg: 'gap-6',
};

export function Grid({
  columns = 1,
  gap = 'md',
  align,
  className = '',
  children,
}: GridProps) {
  return (
    <div
      className={cn('grid', columnsMap[columns], gapMap[gap], className)}
      style={{ alignItems: align }}
    >
      {children}
    </div>
  );
}
