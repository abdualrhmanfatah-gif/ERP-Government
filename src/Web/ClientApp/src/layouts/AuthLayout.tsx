interface AuthLayoutProps {
  children: React.ReactNode;
}

export function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-[var(--color-surface-container-lowest)] p-8">
      <div className="mb-8 text-center">
        <h1 className="text-3xl font-bold text-[var(--color-primary)]">ERP Government</h1>
      </div>
      {children}
    </main>
  );
}
