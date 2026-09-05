import type { ReactNode, CSSProperties } from 'react';
import { cn } from '@/lib/utils';

type StackDirection = 'row' | 'column';
type StackGap = 'xs' | 'sm' | 'md' | 'lg';

interface StackProps {
  direction?: StackDirection;
  gap?: StackGap;
  wrap?: boolean;
  align?: CSSProperties['alignItems'];
  justify?: CSSProperties['justifyContent'];
  className?: string;
  children: ReactNode;
}

const gapMap: Record<StackGap, string> = {
  xs: 'gap-1',
  sm: 'gap-2',
  md: 'gap-4',
  lg: 'gap-6',
};

const directionMap: Record<StackDirection, string> = {
  row: 'flex-row',
  column: 'flex-col',
};

export function Stack({
  direction = 'column',
  gap = 'md',
  wrap = false,
  align,
  justify,
  className = '',
  children,
}: StackProps) {
  return (
    <div
      className={cn('flex', directionMap[direction], gapMap[gap], wrap && 'flex-wrap', className)}
      style={{ alignItems: align, justifyContent: justify }}
    >
      {children}
    </div>
  );
}
