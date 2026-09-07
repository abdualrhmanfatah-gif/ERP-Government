import { useMemo } from 'react';
import { ResponsiveBar } from '@nivo/bar';
import { useMonthlyBudgetTrends } from '@/features/dashboard/hooks/useMonthlyBudgetTrends';
import { Loading } from '@/components/ui/Loading';
import { ErrorState } from '@/components/ui/ErrorState';
import { EmptyState } from '@/components/ui/EmptyState';
import { resolveCssVars } from '@/features/dashboard/utils/chartColors';

function formatCurrency(value: number): string {
  if (value >= 1000) {
    return `${(value / 1000).toFixed(0)}k`;
  }
  return value.toString();
}

const CHART_COLORS = resolveCssVars(['var(--chart-1)', 'var(--chart-3)']);

export function BudgetVsActualChart() {
  const { data, isLoading, error, refetch } = useMonthlyBudgetTrends();

  const chartData = useMemo(() => {
    if (!data) return [];
    return data.map((item) => ({
      month: item.month,
      allocated: item.allocated,
      spent: item.spent,
    }));
  }, [data]);

  if (isLoading) {
    return (
      <div className="h-80 flex items-center justify-center">
        <Loading text="جاري تحميل البيانات…" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="h-80 flex items-center justify-center">
        <ErrorState message={error.message} onRetry={refetch} />
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="h-80 flex items-center justify-center">
        <EmptyState message="لا توجد بيانات متاحة" />
      </div>
    );
  }

  return (
    <div className="h-80" dir="ltr">
      <ResponsiveBar
        data={chartData}
        keys={['allocated', 'spent']}
        indexBy="month"
        groupMode="grouped"
        margin={{ top: 20, right: 20, bottom: 50, left: 60 }}
        padding={0.3}
        valueScale={{ type: 'linear' }}
        indexScale={{ type: 'band', round: true }}
        colors={CHART_COLORS}
        borderRadius={4}
        axisBottom={{
          tickSize: 5,
          tickPadding: 5,
          tickRotation: 0,
          legend: '',
          legendPosition: 'middle',
          legendOffset: 32,
        }}
        axisLeft={{
          tickSize: 5,
          tickPadding: 5,
          tickRotation: 0,
          legend: '',
          legendPosition: 'middle',
          legendOffset: -40,
          format: (value) => formatCurrency(value as number),
        }}
        enableLabel={false}
        legends={[
          {
            dataFrom: 'keys',
            anchor: 'top-left',
            direction: 'row',
            justify: false,
            translateX: 0,
            translateY: -20,
            itemsSpacing: 2,
            itemWidth: 100,
            itemHeight: 20,
            itemDirection: 'left-to-right',
            symbolSize: 12,
            symbolShape: 'square',
            data: [
              { id: 'allocated', label: 'Allocated', color: CHART_COLORS[0] },
              { id: 'spent', label: 'Spent', color: CHART_COLORS[1] },
            ],
          },
        ]}
        theme={{
          text: {
            fill: 'var(--color-on-surface-variant)',
            fontSize: 12,
          },
          grid: {
            line: {
              stroke: 'var(--color-border-container)',
            },
          },
          tooltip: {
            container: {
              background: 'var(--color-surface)',
              border: '1px solid var(--color-border-container)',
              borderRadius: '8px',
              boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
            },
          },
        }}
        tooltip={({ id, value, color }) => (
          <div
            style={{
              padding: '8px 12px',
              background: 'var(--color-surface)',
              border: '1px solid var(--color-border-container)',
              borderRadius: '8px',
              boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <div
                style={{
                  width: '12px',
                  height: '12px',
                  borderRadius: '2px',
                  backgroundColor: color,
                }}
              />
              <span style={{ color: 'var(--color-on-surface)', fontWeight: 500 }}>
                {id === 'allocated' ? 'Allocated' : 'Spent'}: {formatCurrency(value as number)}
              </span>
            </div>
          </div>
        )}
      />
    </div>
  );
}
