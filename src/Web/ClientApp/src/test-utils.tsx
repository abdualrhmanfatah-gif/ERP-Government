import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { type ReactElement, type ReactNode } from 'react';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
  },
});

export function TestWrapper({ children }: { children: ReactNode }): ReactElement {
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
