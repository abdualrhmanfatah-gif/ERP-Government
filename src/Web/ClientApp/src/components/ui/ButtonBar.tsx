import type { ReactNode } from 'react';
import { Button } from './Button';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

interface ToolbarAction {
  key: string;
  label: string;
  icon?: ReactNode;
  variant?: 'primary' | 'secondary' | 'ghost' | 'destructive';
  disabled?: boolean;
  disabledReason?: string;
  onClick: () => void;
}

interface ButtonBarProps {
  actions: ToolbarAction[];
}

export function ButtonBar({ actions }: ButtonBarProps) {
  return (
    <TooltipProvider>
      <div
        role="toolbar"
        aria-label="إجراءات الصفحة"
        className="flex items-center gap-2 flex-wrap"
      >
        {actions.map((action) => {
          const button = (
            <Button
              key={action.key}
              variant={action.variant ?? 'outline'}
              disabled={action.disabled}
              onClick={action.onClick}
              aria-label={action.label}
            >
              {action.icon ? (
                <span aria-hidden="true">{action.icon}</span>
              ) : null}
              {action.label}
            </Button>
          );

          if (action.disabled && action.disabledReason) {
            return (
              <Tooltip key={action.key}>
                <TooltipTrigger render={<span />}>{button}</TooltipTrigger>
                <TooltipContent>
                  <p>{action.disabledReason}</p>
                </TooltipContent>
              </Tooltip>
            );
          }

          return button;
        })}
      </div>
    </TooltipProvider>
  );
}
