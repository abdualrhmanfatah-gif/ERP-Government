import { Sun, Moon, Laptop } from 'lucide-react';
import { useTheme } from './ThemeContext';
import { Button } from '@/components/ui';

const icons = {
  auto: <Laptop size={22} strokeWidth={2} />,
  light: <Sun size={22} strokeWidth={2} />,
  dark: <Moon size={22} strokeWidth={2} />,
};

const labels: Record<string, string> = {
  auto: 'تلقائي',
  light: 'فاتح',
  dark: 'داكن',
};

export function ThemeToggle() {
  const { theme, cycle } = useTheme();

  return (
    <Button
      variant="header"
      size="icon"
      onClick={cycle}
      aria-label={labels[theme as string] ?? 'تبديل السمة'}
      title={labels[theme as string]}
    >
      {icons[theme as keyof typeof icons] ?? icons.auto}
    </Button>
  );
}
