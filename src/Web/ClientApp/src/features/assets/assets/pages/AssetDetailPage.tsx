import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pencil, Power, PowerOff } from 'lucide-react';
import { Page, Button, ConfirmDialog, EmptyState, StatusBadge, Badge } from '@/components/ui';
import { useAssetById, useDeactivateAsset } from '../hooks/useAssets';
import { acquisitionTypeLabels } from '../shared/types';
import { getAssetStatusBadge } from '../../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { AssetForm } from '../components/AssetForm';

export function AssetDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: asset, isLoading, error, refetch } = useAssetById(Number(id));
  const deactivateAsset = useDeactivateAsset();
  const [confirmDeactivate, setConfirmDeactivate] = useState(false);
  const [confirmActivate, setConfirmActivate] = useState(false);

  if (isLoading) {
    return <Page title="" loading>{null}</Page>;
  }

  if (error) {
    return (
      <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>
        {null}
      </Page>
    );
  }

  if (!asset) {
    return (
      <Page title="" onBack={() => navigate('/assets')}>
        <EmptyState message="الأصل غير موجود" />
      </Page>
    );
  }

  const badge = getAssetStatusBadge(asset.status);
  const canDeactivate = ['Active', 'UnderMaintenance'].includes(asset.status);
  const canActivate = asset.status === 'Draft';

  async function handleDeactivate() {
    if (!asset) return;
    try {
      await deactivateAsset.mutateAsync({ id: asset.id, rowVersion: asset.rowVersion });
      setConfirmDeactivate(false);
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  async function handleActivate() {
    if (!asset) return;
    try {
      await deactivateAsset.mutateAsync({ id: asset.id, rowVersion: asset.rowVersion });
      setConfirmActivate(false);
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  return (
    <Page
      title={asset.name}
      description={
        <span className="flex flex-wrap items-center gap-3">
          <span dir="ltr" className="tabular-nums">{asset.code}</span>
          <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>
          <Badge variant={asset.isActive ? 'success' : 'default'}>
            {asset.isActive ? 'مُفعّل: نعم' : 'مُفعّل: لا'}
          </Badge>
          <Badge variant="outline">{acquisitionTypeLabels[asset.acquisitionType as keyof typeof acquisitionTypeLabels] ?? asset.acquisitionType}</Badge>
        </span>
      }
      onBack={() => navigate('/assets')}
      maxWidth="full"
      actions={
        <div className="flex gap-2">
          <Button
            variant="primary"
            icon={<Pencil size={14} />}
            onClick={() => navigate(`/assets/${asset.id}/edit`)}
          >
            تعديل
          </Button>
          {canDeactivate && (
            <Button
              variant="destructive"
              icon={<PowerOff size={14} />}
              onClick={() => setConfirmDeactivate(true)}
            >
              تعطيل
            </Button>
          )}
          {canActivate && (
            <Button
              variant="outline"
              icon={<Power size={14} />}
              onClick={() => setConfirmActivate(true)}
            >
              تفعيل
            </Button>
          )}
        </div>
      }
    >
      <AssetForm mode="detail" initialData={asset} />

      <ConfirmDialog
        open={confirmDeactivate}
        onClose={() => setConfirmDeactivate(false)}
        title="تعطيل الأصل"
        message={`هل أنت متأكد من تعطيل الأصل "${asset.name}"؟`}
        confirmLabel="تعطيل"
        onConfirm={handleDeactivate}
        destructive
        loading={deactivateAsset.isPending}
      />

      <ConfirmDialog
        open={confirmActivate}
        onClose={() => setConfirmActivate(false)}
        title="تفعيل الأصل"
        message={`هل أنت متأكد من تفعيل الأصل "${asset.name}"؟`}
        confirmLabel="تفعيل"
        onConfirm={handleActivate}
        loading={deactivateAsset.isPending}
      />
    </Page>
  );
}
