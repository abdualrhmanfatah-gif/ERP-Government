import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react';

const STORAGE_KEY = 'erpTheme';

type Theme = 'auto' | 'light' | 'dark';

interface ThemeContextValue {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  cycle: () => void;
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: 'auto',
  setTheme: () => {},
  cycle: () => {},
});

export function useTheme(): ThemeContextValue {
  return useContext(ThemeContext);
}

function applyTheme(theme: Theme): void {
  const root = document.documentElement;
  if (theme === 'dark') {
    root.classList.add('dark');
  } else {
    root.classList.remove('dark');
  }
  root.removeAttribute('data-theme');
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>(
    () => (localStorage.getItem(STORAGE_KEY) as Theme) || 'auto'
  );

  useEffect(() => {
    applyTheme(theme);
    localStorage.setItem(STORAGE_KEY, theme);
  }, [theme]);

  const cycle = useCallback(() => {
    setTheme((prev) => {
      if (prev === 'auto') return 'light';
      if (prev === 'light') return 'dark';
      return 'auto';
    });
  }, []);

  return (
    <ThemeContext.Provider value={{ theme, setTheme, cycle }}>
      {children}
    </ThemeContext.Provider>
  );
}
