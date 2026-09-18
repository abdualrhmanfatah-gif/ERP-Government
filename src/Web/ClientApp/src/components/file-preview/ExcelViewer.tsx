import { useEffect, useState } from 'react';
import * as XLSX from 'xlsx';
import { Loading } from '@/components/ui/Loading';
import { Alert } from '@/components/ui/Alert';

interface ExcelViewerProps {
  blob: Blob;
}

interface SheetData {
  name: string;
  headers: string[];
  rows: (string | number)[][];
}

export function ExcelViewer({ blob }: ExcelViewerProps) {
  const [sheets, setSheets] = useState<SheetData[]>([]);
  const [activeSheet, setActiveSheet] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    blob
      .arrayBuffer()
      .then((buffer) => {
        const workbook = XLSX.read(buffer, { type: 'array' });
        const parsed = workbook.SheetNames.map((name) => {
          const sheet = workbook.Sheets[name];
          const json = XLSX.utils.sheet_to_json<(string | number)[]>(sheet, { header: 1 });
          const [headers, ...rows] = json;
          return {
            name,
            headers: (headers ?? []).map(String),
            rows: rows.filter((r) => r.length > 0) as (string | number)[][],
          };
        });
        setSheets(parsed);
      })
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'فشل قراءة ملف Excel');
      })
      .finally(() => setIsLoading(false));
  }, [blob]);

  if (isLoading) return <Loading />;
  if (error) return <Alert variant="error">{error}</Alert>;
  if (sheets.length === 0) return <Alert variant="warning">الملف فارغ</Alert>;

  const current = sheets[activeSheet];

  return (
    <div className="overflow-auto max-h-[70vh]">
      {sheets.length > 1 && (
        <div className="flex gap-1 mb-3 border-b border-[var(--color-container-border)] pb-2">
          {sheets.map((sheet, i) => (
            <button
              key={sheet.name}
              type="button"
              className={`px-3 py-1 text-sm rounded-t border border-b-0 cursor-pointer transition-colors ${
                i === activeSheet
                  ? 'bg-[var(--color-primary)] text-white border-[var(--color-primary)]'
                  : 'bg-transparent text-[var(--color-on-surface-variant)] border-transparent hover:bg-[var(--color-surface-container-low)]'
              }`}
              onClick={() => setActiveSheet(i)}
            >
              {sheet.name}
            </button>
          ))}
        </div>
      )}

      <table className="w-full text-sm border-collapse">
        <thead>
          <tr>
            {current.headers.map((h, i) => (
              <th
                key={`h-${i}`}
                className="px-3 py-2 text-right font-semibold bg-[var(--color-surface-container-low)] border border-[var(--color-container-border)] sticky top-0"
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {current.rows.map((row, ri) => (
            <tr key={`r-${ri}`}>
              {current.headers.map((_, ci) => (
                <td
                  key={`c-${ri}-${ci}`}
                  className="px-3 py-1.5 border border-[var(--color-container-border)]"
                >
                  {row[ci] ?? ''}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
