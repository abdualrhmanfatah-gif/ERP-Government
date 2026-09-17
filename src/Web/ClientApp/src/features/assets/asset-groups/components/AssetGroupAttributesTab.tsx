import { useEffect, useMemo, useState } from 'react';
import { Button, Card, ConfirmDialog, Input, Switch, Combobox } from '@/components/ui';
import { Save, Trash2, Plus } from 'lucide-react';
import { useSaveGroupAttributeBindings } from '../hooks/useAssetGroups';
import { useAssetAttributesList } from '../../asset-attributes/hooks/useAssetAttributes';
import { dataTypeLabels } from '../../asset-attributes/shared/types';
import type { AssetGroupAttributeBinding } from '../shared/types';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { notify } from '@/components/ui/Toast';

interface BindingRow {
  assetAttributeDefinitionId: number;
  code: string;
  name: string;
  dataType: string;
  isRequired: boolean;
  sortOrder: number | null;
}

interface AssetGroupAttributesTabProps {
  groupId: number;
  initialBindings: AssetGroupAttributeBinding[];
}

function toRows(bindings: AssetGroupAttributeBinding[]): BindingRow[] {
  return bindings.map(b => ({
    assetAttributeDefinitionId: b.assetAttributeDefinitionId,
    code: b.code,
    name: b.name,
    dataType: b.dataType,
    isRequired: b.isRequired,
    sortOrder: b.sortOrder,
  }));
}

export function AssetGroupAttributesTab({ groupId, initialBindings }: AssetGroupAttributesTabProps) {
  const { data: allDefinitions } = useAssetAttributesList({ isActive: true, pageSize: 200 });
  const saveMutation = useSaveGroupAttributeBindings();

  const [rows, setRows] = useState<BindingRow[]>(() => toRows(initialBindings));
  const [dirty, setDirty] = useState(false);
  const [removeTarget, setRemoveTarget] = useState<BindingRow | null>(null);
  const [addDefId, setAddDefId] = useState<string>('');

  useEffect(() => {
    if (!dirty) setRows(toRows(initialBindings));
  }, [initialBindings, dirty]);

  const boundDefIds = useMemo(() => new Set(rows.map(b => b.assetAttributeDefinitionId)), [rows]);
  const unboundDefs = useMemo(
    () => (allDefinitions?.items ?? []).filter(d => !boundDefIds.has(d.id)),
    [allDefinitions, boundDefIds]
  );
  const addOptions = useMemo(
    () => unboundDefs.map(d => ({ value: String(d.id), label: `${d.code} - ${d.name}` })),
    [unboundDefs]
  );

  function handleAdd() {
    if (!addDefId) return;
    const def = unboundDefs.find(d => d.id === Number(addDefId));
    if (!def) return;
    setRows(prev => [...prev, {
      assetAttributeDefinitionId: def.id,
      code: def.code,
      name: def.name,
      dataType: def.attributeDataType,
      isRequired: false,
      sortOrder: null,
    }]);
    setAddDefId('');
    setDirty(true);
  }

  function confirmRemove() {
    if (!removeTarget) return;
    setRows(prev => prev.filter(b => b.assetAttributeDefinitionId !== removeTarget.assetAttributeDefinitionId));
    setRemoveTarget(null);
    setDirty(true);
  }

  function handleToggleRequired(defId: number) {
    setRows(prev => prev.map(b =>
      b.assetAttributeDefinitionId === defId ? { ...b, isRequired: !b.isRequired } : b
    ));
    setDirty(true);
  }

  function handleSortOrderChange(defId: number, value: string) {
    setRows(prev => prev.map(b =>
      b.assetAttributeDefinitionId === defId ? { ...b, sortOrder: value === '' ? null : Number(value) } : b
    ));
    setDirty(true);
  }

  async function handleSave() {
    if (rows.length === 0) {
      notify({ type: 'warning', title: 'يجب تحديد مواصفة واحدة على الأقل' });
      return;
    }
    try {
      await saveMutation.mutateAsync({
        groupId,
        bindings: rows.map(b => ({
          assetAttributeDefinitionId: b.assetAttributeDefinitionId,
          isRequired: b.isRequired,
          sortOrder: b.sortOrder,
        })),
      });
      setDirty(false);
      notify({ type: 'success', title: 'تم حفظ ربط المواصفات بنجاح' });
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <Card>
        <div className="flex flex-wrap items-end gap-3">
          <div className="min-w-56 flex-1">
            <Combobox
              label="إضافة مواصفة"
              options={addOptions}
              value={addDefId || undefined}
              onChange={(v) => setAddDefId(v ?? '')}
              placeholder="اختر مواصفة لإضافتها..."
              searchPlaceholder="بحث بالكود أو الاسم..."
            />
          </div>
          <Button
            variant="outline"
            icon={<Plus size={16} />}
            onClick={handleAdd}
            disabled={!addDefId}
            data-testid="group-attr-add"
          >
            إضافة مواصفة
          </Button>
          <div className="flex-1" />
          <Button
            variant="primary"
            icon={<Save size={16} />}
            loading={saveMutation.isPending}
            onClick={handleSave}
            data-testid="group-attr-save"
          >
            حفظ الربط
          </Button>
        </div>
      </Card>

      <Card padding="none" className="overflow-hidden" data-testid="group-attr-table">
        {rows.length === 0 ? (
          <div className="p-4 text-center text-[var(--color-on-surface-variant)]">
            لا توجد مواصفات مربوطة بهذه المجموعة بعد
          </div>
        ) : (
          <>
            <div className="flex items-center gap-2 bg-[var(--color-primary)] px-3 py-2 text-xs font-semibold text-[var(--color-on-primary)]">
              <span className="flex-1">المواصفة</span>
              <span className="w-24 shrink-0">النوع</span>
              <span className="w-20 shrink-0 text-center">مطلوب</span>
              <span className="w-24 shrink-0 text-center">الترتيب</span>
              <span className="w-12 shrink-0"></span>
            </div>
            {rows.map(b => (
              <div
                key={b.assetAttributeDefinitionId}
                className="flex items-center gap-2 border-b border-[var(--color-container-border)] bg-[var(--color-surface)] px-3 py-1.5 transition-colors last:border-b-0 hover:bg-[var(--color-surface-container-low)]"
              >
                <span className="flex-1 min-w-0">
                  <span className="block truncate font-medium">{b.name}</span>
                  <span className="text-xs text-[var(--color-on-surface-variant)] font-mono" dir="ltr">{b.code}</span>
                </span>
                <span className="w-24 shrink-0">
                  <span className="inline-flex items-center rounded-full border border-[var(--color-outline)] px-2 py-0.5 text-xs font-medium">
                    {dataTypeLabels[b.dataType as keyof typeof dataTypeLabels] ?? b.dataType}
                  </span>
                </span>
                <span className="w-20 shrink-0 flex justify-center" data-testid={`group-attr-required-${b.assetAttributeDefinitionId}`}>
                  <Switch
                    checked={b.isRequired}
                    onChange={() => handleToggleRequired(b.assetAttributeDefinitionId)}
                    size="sm"
                  />
                </span>
                <span className="w-24 shrink-0">
                  <Input
                    type="number"
                    value={b.sortOrder ?? ''}
                    onChange={(e) => handleSortOrderChange(b.assetAttributeDefinitionId, e.target.value)}
                    className="w-full text-center"
                    data-testid={`group-attr-sort-${b.assetAttributeDefinitionId}`}
                  />
                </span>
                <span className="w-12 shrink-0 flex justify-center">
                  <Button
                    variant="ghost"
                    size="icon-xs"
                    className="text-[var(--color-error)]"
                    onClick={() => setRemoveTarget(b)}
                    aria-label="إزالة المواصفة"
                    data-testid={`group-attr-remove-${b.assetAttributeDefinitionId}`}
                  >
                    <Trash2 size={14} />
                  </Button>
                </span>
              </div>
            ))}
          </>
        )}
      </Card>

      <ConfirmDialog
        open={!!removeTarget}
        onClose={() => setRemoveTarget(null)}
        title="إزالة المواصفة"
        message={`هل أنت متأكد من إزالة المواصفة "${removeTarget?.name}" من هذه المجموعة؟ القيم الموجودة على الأصول ستحتفظ بقيمها.`}
        confirmLabel="إزالة"
        onConfirm={confirmRemove}
        destructive
      />
    </div>
  );
}
