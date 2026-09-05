import { useMemo, useState } from 'react';
import { Pie } from '@nivo/pie';
import { useExpenseBreakdown } from '../hooks/useExpenseBreakdown';
import { Loading } from '../../../components/ui/Loading';
import { ErrorState } from '../../../components/ui/ErrorState';
import { EmptyState } from '../../../components/ui/EmptyState';
import { resolveCssVars } from '../utils/chartColors';

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

const PIE_COLORS = resolveCssVars([
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
  'var(--chart-6)',
]);

export function ExpenseBreakdownChart() {
  const { data, isLoading, error, refetch } = useExpenseBreakdown();
  const [activeId, setActiveId] = useState<string | null>(null);

  const chartData = useMemo(() => {
    if (!data) return [];
    return data.map((item, index) => ({
      id: item.id,
      label: item.label,
      value: item.value,
      color: PIE_COLORS[index % PIE_COLORS.length],
    }));
  }, [data]);

  const total = useMemo(() => {
    return data?.reduce((sum, item) => sum + item.value, 0) ?? 0;
  }, [data]);

  if (isLoading) {
    return (
      <div className="flex h-80 items-center justify-center">
        <Loading text="جاري التحميل…" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex h-80 items-center justify-center">
        <ErrorState message={error.message} onRetry={refetch} />
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="flex h-80 items-center justify-center">
        <EmptyState message="لا توجد بيانات" />
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center">
      {/* Pie Chart — explicit 280×280 */}
      <div className="relative flex-shrink-0" dir="ltr">
        <Pie
          width={280}
          height={280}
          data={chartData}
          valueFormat={formatCurrency}
          margin={{ top: 10, right: 10, bottom: 10, left: 10 }}
          innerRadius={0.65}
          padAngle={2}
          cornerRadius={4}
          activeOuterRadiusOffset={8}
          activeId={activeId}
          onMouseEnter={(node) => setActiveId(String(node.id))}
          onMouseLeave={() => setActiveId(null)}
          borderWidth={1}
          borderColor={{ from: 'color', modifiers: [['darker', 0.2]] }}
          colors={chartData.map((item) => item.color)}
          enableArcLabels={true}
          arcLabel="formattedValue"
          arcLabelsSkipAngle={20}
          arcLabelsTextColor="#ffffff"
          enableArcLinkLabels={false}
          layers={[
            'arcs',
            'arcLabels',
            ({ centerX, centerY }: { centerX: number; centerY: number }) => (
              <g>
                <text
                  x={centerX}
                  y={centerY - 8}
                  textAnchor="middle"
                  dominantBaseline="central"
                  style={{
                    fill: 'var(--color-on-surface)',
                    fontSize: '22px',
                    fontWeight: 700,
                  }}
                >
                  {formatCurrency(total)}
                </text>
                <text
                  x={centerX}
                  y={centerY + 16}
                  textAnchor="middle"
                  dominantBaseline="central"
                  style={{
                    fill: 'var(--color-on-surface-variant)',
                    fontSize: '13px',
                    fontWeight: 500,
                  }}
                >
                  الإجمالي
                </text>
              </g>
            ),
          ]}
          tooltip={({ datum }) => (
            <div className="flex items-center gap-2 rounded-lg border border-border-container bg-surface px-3 py-2 shadow-md">
              <div
                className="h-3 w-3 rounded-sm"
                style={{ backgroundColor: datum.color }}
              />
              <span className="text-sm font-medium" style={{ color: 'var(--color-on-surface)' }}>
                {datum.label}: {datum.formattedValue}
              </span>
            </div>
          )}
        />
      </div>
    </div>
  );
}