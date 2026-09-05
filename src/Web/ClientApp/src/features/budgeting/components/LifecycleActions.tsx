import { Button } from '@/components/ui/Button';

export interface LifecycleAction {
  key: string;
  label: string;
  permission?: string;
  confirmMessage?: string;
}

interface LifecycleActionsProps {
  actions: LifecycleAction[];
  can: (permission?: string) => boolean;
  pendingKey?: string | null;
  onAction: (key: string) => void;
}

export function LifecycleActions({ actions, can, pendingKey, onAction }: LifecycleActionsProps) {
  const visible = actions.filter((a) => can(a.permission));

  if (visible.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-2" role="group" aria-label="إجراءات دورة الحياة">
      {visible.map((action) => (
        <Button
          key={action.key}
          variant="primary"
          size="sm"
          disabled={pendingKey === action.key}
          onClick={() => {
            if (action.confirmMessage && !window.confirm(action.confirmMessage)) return;
            onAction(action.key);
          }}
        >
          {action.label}
        </Button>
      ))}
    </div>
  );
}
