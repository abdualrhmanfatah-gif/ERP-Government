import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Page, Button, Card, ConfirmDialog, EmptyState, StatusBadge, Tabs, Badge, DataGrid } from '@/components/ui';
import { Pencil, Power, PowerOff, Coins, BookOpen, History, Layers, Tag } from 'lucide-react';
import { useAssetGroupDetail, useToggleAssetGroupActive, useAssetGroupsList } from '../hooks/useAssetGroups';
import { AssetGroupAttributesTab } from '../components/AssetGroupAttributesTab';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { assetCategoryLabels, type AssetGroup } from '../shared/types';
import { getDepreciationMethodLabel } from '../../shared/depreciation-method';
import { getActiveBadge } from '../../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';

interface AuditRow {
  id: string;
  action: string;
  user: string;
  date: string;
}

function ReadOnlyField({ label, value, ltr, className }: { label: string; value: string | number | null | undefined; ltr?: boolean; className?: string }) {
  const text = value === null || value === undefined || value === '' ? '—' : String(value);
  return (
    <div className={['flex flex-col gap-1', className ?? ''].filter(Boolean).join(' ')}>
      <span className="text-label-md text-[var(--color-on-surface)]">{label}</span>
      <div
        dir={ltr ? 'ltr' : undefined}
        title={text}
        className={[
          'flex h-[var(--density-compact-control-height)] items-center rounded-lg border-2 border-[var(--color-container-border)] bg-[var(--color-surface-container-low)] px-3 text-sm text-[var(--color-on-surface)]',
          ltr ? 'tabular-nums font-mono' : '',
        ].filter(Boolean).join(' ')}
      >
        <span className="truncate">{text}</span>
      </div>
    </div>
  );
}

export default function AssetGroupDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const groupId = Number(id);

  const { data: group, isLoading, error, refetch } = useAssetGroupDetail(groupId);
  const { data: childGroups = [], isLoading: childrenLoading, error: childrenError, refetch: refetchChildren } = useAssetGroupsList({ parentId: groupId });
  const { data: accounts = [] } = useAccountsList({ isActive: true });
  const toggleMutation = useToggleAssetGroupActive();
  const [showDeactivateDialog, setShowDeactivateDialog] = useState(false);
  const [showActivateDialog, setShowActivateDialog] = useState(false);

  const accountMap = new Map((accounts ?? []).map(a => [a.id, `${a.code} - ${a.name}`]));

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

  if (!group) {
    return (
      <Page title="" onBack={() => navigate('/assets/asset-groups')}>
        <EmptyState message="المجموعة غير موجودة" />
      </Page>
    );
  }

  const badge = getActiveBadge(group.isActive);

  const accountValue = (accountId: number | null | undefined) => {
    if (!accountId) return '—';
    return accountMap.get(accountId) ?? String(accountId);
  };

  async function handleDeactivate() {
    if (!group) return;
    try {
      await toggleMutation.mutateAsync({ id: group.id, activate: false, rowVersion: group.rowVersion });
      setShowDeactivateDialog(false);
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  async function handleActivate() {
    if (!group) return;
    try {
      await toggleMutation.mutateAsync({ id: group.id, activate: true, rowVersion: group.rowVersion });
      setShowActivateDialog(false);
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  return (
    <Page
      title={group.name}
      description={
        <span className="flex flex-wrap items-center gap-3">
          <span dir="ltr" className="tabular-nums">{group.code}</span>
          <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>
          <Badge variant="outline">{assetCategoryLabels[group.assetCategory] ?? group.assetCategory}</Badge>
        </span>
      }
      onBack={() => navigate('/assets/asset-groups')}
      maxWidth="xl"
      actions={
        <div className="flex gap-2">
          <Button
            variant="primary"
            icon={<Pencil size={14} />}
            onClick={() => navigate(`/assets/asset-groups/${group.id}/edit`)}
          >
            تعديل
          </Button>
          {group.isActive ? (
            <Button
              variant="destructive"
              icon={<PowerOff size={14} />}
              onClick={() => setShowDeactivateDialog(true)}
            >
              تعطيل
            </Button>
          ) : (
            <Button
              variant="outline"
              icon={<Power size={14} />}
              onClick={() => setShowActivateDialog(true)}
            >
              تفعيل
            </Button>
          )}
        </div>
      }
    >
      <div className="flex flex-col gap-4">
        <Card>
          <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">المعلومات الأساسية</h3>
          <div className="flex flex-wrap items-end gap-3">
            <ReadOnlyField className="w-36 shrink-0" label="الكود" value={group.code} ltr />
            <ReadOnlyField className="min-w-56 flex-1" label="الاسم" value={group.name} />
            <ReadOnlyField className="min-w-44 flex-1" label="المجموعة الأب" value={group.parentName} />
            <ReadOnlyField className="min-w-64 flex-[2]" label="الوصف" value={group.description} />
          </div>
        </Card>

        <Tabs
          tabs={[
            {
              key: 'children',
              label: `المجموعات الفرعية (${childGroups.length})`,
              icon: <Layers size={15} />,
              content: (
                <Card>
                  <DataGrid<AssetGroup>
                    data={childGroups}
                    rowKey={(row) => row.id}
                    onRowClick={(row) => navigate(`/assets/asset-groups/${row.id}`)}
                    loading={childrenLoading}
                    error={childrenError ? getQueryErrorMessage(childrenError) : undefined}
                    onRetry={() => refetchChildren()}
                    emptyMessage="لا توجد مجموعات فرعية"
                    columns={[
                      {
                        id: 'code',
                        accessorKey: 'code',
                        header: 'الكود',
                        width: 120,
                        cell: (row) => <span dir="ltr" className="tabular-nums font-mono">{row.code}</span>,
                      },
                      { id: 'name', accessorKey: 'name', header: 'الاسم' },
                      {
                        id: 'assetCategory',
                        accessorKey: 'assetCategory',
                        header: 'الفئة',
                        cell: (row) => (
                          <Badge variant="outline">{assetCategoryLabels[row.assetCategory] ?? row.assetCategory}</Badge>
                        ),
                      },
                      {
                        id: 'isActive',
                        accessorKey: 'isActive',
                        header: 'الحالة',
                        width: 130,
                        cell: (row) => {
                          const childBadge = getActiveBadge(row.isActive);
                          return <StatusBadge variant={childBadge.variant}>{childBadge.label}</StatusBadge>;
                        },
                      },
                    ]}
                  />
                </Card>
              ),
            },
            {
              key: 'attributes',
              label: `المواصفات (${group.attributeBindings?.length ?? 0})`,
              icon: <Tag size={15} />,
              content: (
                <AssetGroupAttributesTab
                  groupId={group.id}
                  initialBindings={group.attributeBindings ?? []}
                />
              ),
            },
            {
              key: 'depreciation',
              label: 'معلمات الإهلاك',
              icon: <Coins size={15} />,
              content: (
                <Card>
                  <div className="flex flex-wrap items-end gap-3">
                    <ReadOnlyField
                      className="min-w-48 flex-1"
                      label="طريقة الإهلاك"
                      value={getDepreciationMethodLabel(group.depreciationMethod)}
                    />
                    <ReadOnlyField
                      className="min-w-36 flex-1"
                      label="العمر الإنتاجي (سنوات)"
                      value={group.defaultUsefulLifeYears}
                    />
                    <ReadOnlyField
                      className="min-w-36 flex-1"
                      label="نسبة القيمة التخريدية (%)"
                      value={group.residualValuePercentage}
                    />
                    <ReadOnlyField
                      className="min-w-36 flex-1"
                      label="معدل الإهلاك (%)"
                      value={group.depreciationRate}
                    />
                    <ReadOnlyField
                      className="w-28 shrink-0"
                      label="قابل للإهلاك"
                      value={group.isDepreciable ? 'نعم' : 'لا'}
                    />
                  </div>
                </Card>
              ),
            },
            {
              key: 'accounts',
              label: 'الحسابات المحاسبية',
              icon: <BookOpen size={15} />,
              content: (
                <Card>
                  <div className="flex flex-wrap items-end gap-3">
                    <ReadOnlyField className="min-w-56 flex-1" label="حساب الأصول" value={accountValue(group.assetAccountId)} />
                    <ReadOnlyField className="min-w-56 flex-1" label="حساب مجمع الإهلاك" value={accountValue(group.accumulatedDepreciationAccountId)} />
                    <ReadOnlyField className="min-w-56 flex-1" label="حساب مصروف الإهلاك" value={accountValue(group.depreciationExpenseAccountId)} />
                    <ReadOnlyField className="min-w-56 flex-1" label="حساب التخلص" value={accountValue(group.disposalAccountId)} />
                  </div>
                </Card>
              ),
            },
            {
              key: 'audit',
              label: 'سجل التدقيق',
              icon: <History size={15} />,
              content: (
                <Card>
                  <DataGrid<AuditRow>
                    data={[
                      {
                        id: 'created',
                        action: 'إنشاء',
                        user: group.createdBy ?? '—',
                        date: new Date(group.created).toLocaleDateString('ar'),
                      },
                      {
                        id: 'modified',
                        action: 'آخر تعديل',
                        user: group.lastModifiedBy ?? '—',
                        date: new Date(group.lastModified).toLocaleDateString('ar'),
                      },
                    ]}
                    rowKey={(row) => row.id}
                    columns={[
                      { id: 'action', accessorKey: 'action', header: 'الإجراء' },
                      { id: 'user', accessorKey: 'user', header: 'المستخدم' },
                      { id: 'date', accessorKey: 'date', header: 'التاريخ' },
                    ]}
                  />
                </Card>
              ),
            },
          ]}
        />
      </div>

      <ConfirmDialog
        open={showDeactivateDialog}
        onClose={() => setShowDeactivateDialog(false)}
        title="تعطيل المجموعة"
        message="هل أنت متأكد من تعطيل هذه المجموعة؟ لن تظهر في قوائم اختيار الأصول الجديدة."
        confirmLabel="تعطيل"
        onConfirm={handleDeactivate}
        destructive
        loading={toggleMutation.isPending}
      />

      <ConfirmDialog
        open={showActivateDialog}
        onClose={() => setShowActivateDialog(false)}
        title="تفعيل المجموعة"
        message="هل أنت متأكد من تفعيل هذه المجموعة؟"
        confirmLabel="تفعيل"
        onConfirm={handleActivate}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
