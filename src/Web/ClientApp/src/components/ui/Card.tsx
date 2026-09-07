import type { ReactNode, HTMLAttributes } from 'react';
import { cn } from '@/lib/utils';

type CardVariant = 'default' | 'flat' | 'outlined';
type CardPadding = 'none' | 'sm' | 'md' | 'lg';

interface CardProps extends Omit<HTMLAttributes<HTMLDivElement>, 'children'> {
  variant?: CardVariant;
  padding?: CardPadding;
  as?: 'div' | 'section' | 'article' | 'aside';
  className?: string;
  children: ReactNode;
}

const variantClasses: Record<CardVariant, string> = {
  default: 'bg-[var(--color-surface)] border border-[var(--color-outline-variant)]',
  flat: 'bg-[var(--color-surface)] border border-transparent',
  outlined: 'bg-transparent border border-[var(--color-outline-variant)]',
};

const paddingClasses: Record<CardPadding, string> = {
  none: '',
  sm: 'p-3',
  md: 'p-6',
  lg: 'p-8',
};

export function Card({
  variant = 'default',
  padding = 'md',
  as: Component = 'div',
  className,
  children,
  ...rest
}: CardProps) {
  return (
    <Component
      className={cn(
        'rounded-xl',
        variantClasses[variant],
        paddingClasses[padding],
        className,
      )}
      {...rest}
    >
      {children}
    </Component>
  );
}

export type { CardProps, CardVariant, CardPadding };
