import './shared/utils/patch-fetch'; // must be first — monkey-patches window.fetch for JWT auth
import './styles.scss';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { App } from './App';
import { AuthProvider } from './shared/hooks/useAuth';
import { ThemeProvider } from './components/ThemeContext';
import { ToastProvider } from './components/ui/Toast';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
});

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const root = createRoot(document.getElementById('root')!);

root.render(
  <BrowserRouter basename={baseUrl ?? '/'}>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <ToastProvider />
          <App />
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  </BrowserRouter>
);
