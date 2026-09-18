import { useEffect, useState } from 'react';
import { Loading } from '@/components/ui/Loading';
import { Alert } from '@/components/ui/Alert';

interface TextViewerProps {
  blob: Blob;
}

export function TextViewer({ blob }: TextViewerProps) {
  const [text, setText] = useState<string>('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    blob
      .text()
      .then(setText)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'فشل قراءة الملف النصي');
      })
      .finally(() => setIsLoading(false));
  }, [blob]);

  if (isLoading) return <Loading />;
  if (error) return <Alert variant="error">{error}</Alert>;

  return (
    <div className="overflow-auto max-h-[70vh] bg-[var(--color-surface-container-low)] rounded border border-[var(--color-container-border)] p-4">
      <pre className="text-sm font-mono whitespace-pre-wrap break-words">{text}</pre>
    </div>
  );
}
