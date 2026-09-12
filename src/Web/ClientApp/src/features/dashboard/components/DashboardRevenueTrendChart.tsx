import { useMemo } from 'react';
import { ResponsiveLine } from '@nivo/line';
import { Loading, ErrorState, EmptyState } from '@/components/ui';
import { useMonthlyBudgetTrends } from '../hooks/useMonthlyBudgetTrends';
import { resolveCssVar } from '../utils/chartColors';

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('ar-SA', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

const LINE_COLOR = resolveCssVar('var(--chart-3)');

export function RevenueTrendChart() {
  const { data, isLoading, error, refetch } = useMonthlyBudgetTrends();

  const chartData = useMemo(() => {
    if (!data) return [];
    return [
      {
        id: 'spent',
        color: LINE_COLOR,
        data: data.map((item, index) => ({
          x: index,
          y: item.spent,
          month: item.month,
        })),
      },
    ];
  }, [data]);

  const monthLabels = useMemo(() => {
    if (!data) return [];
    return data.map((item) => item.month);
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
      <ResponsiveLine
        data={chartData}
        margin={{ top: 20, right: 20, bottom: 50, left: 60 }}
        xScale={{ type: 'point' }}
        yScale={{
          type: 'linear',
          min: 'auto',
          max: 'auto',
          stacked: false,
          reverse: false,
        }}
        axisBottom={{
          tickSize: 5,
          tickPadding: 5,
          tickRotation: 0,
          legend: '',
          legendPosition: 'middle',
          legendOffset: 32,
          format: (value) => monthLabels[value as number] ?? '',
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
        enablePoints={true}
        pointSize={6}
        pointColor={{ from: 'series.color' }}
        pointBorderWidth={0}
        pointBorderColor={{ theme: 'background' }}
        useMesh={false}
        colors={[LINE_COLOR]}
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
          crosshair: {
            line: {
              stroke: 'var(--color-outline)',
              strokeWidth: 1,
              strokeDasharray: '4 4',
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
        tooltip={({ point }) => {
          const dataPoint = point.data as { month?: string };
          return (
            <div
              style={{
                padding: '8px 12px',
                background: 'var(--color-surface)',
                border: '1px solid var(--color-border-container)',
                borderRadius: '8px',
                boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
              }}
            >
              <div style={{ color: 'var(--color-on-surface)', fontWeight: 500 }}>
                {dataPoint.month}: {formatCurrency(point.y)}
              </div>
            </div>
          );
        }}
      />
    </div>
  );
}
