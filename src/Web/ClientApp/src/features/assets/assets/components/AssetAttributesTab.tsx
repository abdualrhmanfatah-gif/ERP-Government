import { Card, StatusBadge, Badge } from '@/components/ui';
import { dataTypeLabels } from '../../asset-attributes/shared/types';
import { booleanLabels, getDisabledStatusLabel } from '@/shared/constants/labels';
import type { AssetAttributeValue } from '../shared/types';

function formatValue(v: AssetAttributeValue): string {
  switch (v.dataType) {
    case 'Text':
      return v.textValue || '—';
    case 'Integer':
      return v.integerValue != null ? v.integerValue.toLocaleString('ar') : '—';
    case 'Decimal':
      return v.decimalValue != null ? v.decimalValue.toLocaleString('ar', { maximumFractionDigits: 4 }) : '—';
    case 'Date':
      return v.dateValue ? new Date(v.dateValue).toLocaleDateString('ar') : '—';
    case 'Boolean':
      return v.booleanValue != null
        ? (v.booleanValue ? booleanLabels.true : booleanLabels.false)
        : '—';
    default:
      return '—';
  }
}

interface AssetAttributesTabProps {
  attributeValues: AssetAttributeValue[];
}

export function AssetAttributesTab({ attributeValues }: AssetAttributesTabProps) {
  if (attributeValues.length === 0) {
    return (
      <Card>
        <div className="p-4 text-center text-[var(--color-on-surface-variant)]">
          لا توجد مواصفات لهذا الأصل
        </div>
      </Card>
    );
  }

  return (
    <Card padding="none" className="overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-[var(--color-primary)] text-[var(--color-on-primary)]">
              <th className="px-3 py-2 text-start text-xs font-semibold">المواصفة</th>
              <th className="px-3 py-2 text-start text-xs font-semibold">القيمة</th>
              <th className="px-3 py-2 text-start text-xs font-semibold">النوع</th>
              <th className="px-3 py-2 text-start text-xs font-semibold">الحالة</th>
            </tr>
          </thead>
          <tbody>
            {attributeValues.map((v) => (
              <tr
                key={v.assetAttributeDefinitionId}
                className="border-b border-[var(--color-container-border)] last:border-b-0 hover:bg-[var(--color-surface-container-low)]"
              >
                <td className="px-3 py-2">
                  <span className="block font-medium">{v.name}</span>
                  <span className="text-xs text-[var(--color-on-surface-variant)] font-mono" dir="ltr">{v.code}</span>
                </td>
                <td className="px-3 py-2 tabular-nums" data-testid={`attr-value-${v.assetAttributeDefinitionId}`}>
                  {formatValue(v)}
                </td>
                <td className="px-3 py-2">
                  <Badge variant="outline">{dataTypeLabels[v.dataType] ?? v.dataType}</Badge>
                </td>
                <td className="px-3 py-2" data-testid={`attr-status-${v.assetAttributeDefinitionId}`}>
                  <StatusBadge variant={v.isActive ? 'active' : 'inactive'}>
                    {getDisabledStatusLabel(v.isActive)}
                  </StatusBadge>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
