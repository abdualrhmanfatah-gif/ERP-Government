import { Card } from '@/components/ui';
import { AssetTransactionsTable } from './AssetTransactionsTable';

export function AssetHistoryTab({ assetId }: { assetId: number }) {
  return (
    <Card>
      <h3 className="mb-3 text-base font-semibold">سجل المعاملات</h3>
      <AssetTransactionsTable assetId={assetId} />
    </Card>
  );
}
