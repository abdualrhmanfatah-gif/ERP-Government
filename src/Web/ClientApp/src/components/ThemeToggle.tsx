import { Sun, Moon, Laptop } from 'lucide-react';
import { useTheme } from './ThemeContext';

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
    <button
      type="button"
      className="inline-flex items-center justify-center w-11 h-11 rounded-lg text-white/70 cursor-pointer hover:bg-white/10 hover:text-white transition-colors duration-150"
      onClick={cycle}
      aria-label={labels[theme as string] ?? 'تبديل السمة'}
      title={labels[theme as string]}
    >
      {icons[theme as keyof typeof icons] ?? icons.auto}
    </button>
  );
}
