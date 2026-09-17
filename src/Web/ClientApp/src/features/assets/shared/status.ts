import type { BadgeVariant } from '@/components/ui/StatusBadge';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { assetStatusLabels, type AssetStatus } from '../assets/shared/types';

const ASSET_STATUS_ROLES: Record<AssetStatus, BadgeVariant> = {
  Draft: 'draft',
  Active: 'active',
  UnderMaintenance: 'inactive',
  Disposed: 'closed',
  WrittenOff: 'closed',
};

export function getAssetStatusBadge(status: string | null | undefined): { variant: BadgeVariant; label: string } {
  if (status && status in ASSET_STATUS_ROLES) {
    return {
      variant: ASSET_STATUS_ROLES[status as AssetStatus],
      label: assetStatusLabels[status as AssetStatus],
    };
  }
  return { variant: 'draft', label: status ?? '—' };
}

export function getActiveBadge(isActive: boolean): { variant: BadgeVariant; label: string } {
  return { variant: isActive ? 'active' : 'inactive', label: getActiveStatusLabel(isActive) };
}
