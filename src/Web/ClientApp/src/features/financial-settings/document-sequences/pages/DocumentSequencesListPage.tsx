import { useState } from 'react';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Switch, ConfirmDialog, Input } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { notify } from '@/features/notifications/notify';
import { useDocumentSequencesList, useUpdateDocumentSequence, useDeactivateDocumentSequence } from '../../hooks/useDocumentSequences';
import { resetPolicyLabels, ResetPolicy } from '../../shared/types';

export default function DocumentSequencesListPage() {
  const canUpdate = usePermission(PERMISSIONS.DocumentSequences.Update);
  const canDeactivate = usePermission(PERMISSIONS.DocumentSequences.Deactivate);
  const canManage = canUpdate || canDeactivate;
  const { data: items = [], isLoading } = useDocumentSequencesList();
  const updateMutation = useUpdateDocumentSequence();
  const deactivateMutation = useDeactivateDocumentSequence();

  const [editItem, setEditItem] = useState<{ id: number; name: string; resetPolicy: string } | null>(null);
  const [editName, setEditName] = useState('');
  const [confirmDeactivate, setConfirmDeactivate] = useState<{ id: number; rowVersion: string; name: string } | null>(null);

  function handleEdit(item: typeof items[0]) {
    setEditItem({ id: item.id, name: item.name, resetPolicy: item.resetPolicy });
    setEditName(item.name);
  }

  function saveEdit() {
    if (!editItem) return;
    updateMutation.mutate(
      { id: editItem.id, rowVersion: '', name: editName },
      {
        onSuccess: () => { notify({ type: 'success', title: 'تم التحديث' }); setEditItem(null); },
        onError: () => notify({ type: 'error', title: 'حدث خطأ' }),
      },
    );
  }

  function confirmDeactivateAction() {
    if (!confirmDeactivate) return;
    deactivateMutation.mutate(
      { id: confirmDeactivate.id, rowVersion: confirmDeactivate.rowVersion },
      {
        onSuccess: () => { notify({ type: 'success', title: 'تم التعطيل' }); setConfirmDeactivate(null); },
        onError: () => notify({ type: 'error', title: 'حدث خطأ' }),
      },
    );
  }

  const columns: DataGridColumn<typeof items[0]>[] = [
    {
      header: 'الاسم',
      cell: (row) => editItem?.id === row.id
        ? <Input value={editName} onChange={(e) => setEditName(e.target.value)} className="h-8 text-sm" />
        : <span className="font-medium">{row.name}</span>,
    },
    { header: 'نوع الوثيقة', cell: (row) => <span className="font-mono">{row.documentType}</span> },
    { header: 'الرقم التالي', align: 'left', cell: (row) => <span className="font-mono">{row.currentNumber}</span> },
    { header: 'سياسة إعادة التعيين', cell: (row) => resetPolicyLabels[ResetPolicy[row.resetPolicy as keyof typeof ResetPolicy] as ResetPolicy] ?? row.resetPolicy },
    {
      header: 'الحالة',
      cell: (row) => canManage
        ? <Switch checked={row.isActive} onChange={() => row.isActive ? setConfirmDeactivate({ id: row.id, rowVersion: row.rowVersion, name: row.name }) : undefined} label={row.isActive ? 'نشط' : 'معطل'} />
        : <span className={row.isActive ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}>{row.isActive ? 'نشط' : 'معطل'}</span>,
    },
    ...(canManage ? [{
      header: 'إجراءات',
      cell: (row: typeof items[0]) => editItem?.id === row.id
        ? <div className="flex gap-1"><Button size="xs" onClick={saveEdit} disabled={updateMutation.isPending} className="cursor-pointer">حفظ</Button><Button size="xs" variant="ghost" onClick={() => setEditItem(null)} className="cursor-pointer">إلغاء</Button></div>
        : <Button variant="ghost" size="icon" onClick={() => handleEdit(row)} className="cursor-pointer">✏️</Button>,
    }] : []),
  ];

  return (
    <Page
      title="تسلسل الوثائق"
      description="إدارة أرقام التسلسل للوثائق المالية"
    >
      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        emptyMessage="لا توجد تسلسلات"
        rowKey={(row) => row.id}
      />

      <ConfirmDialog
        open={!!confirmDeactivate}
        onClose={() => setConfirmDeactivate(null)}
        onConfirm={confirmDeactivateAction}
        title="تعطيل التسلسل"
        message={`هل تريد تعطيل تسلسل "${confirmDeactivate?.name}"؟ لن يؤثر على الأرقام שכבר صدرت.`}
        loading={deactivateMutation.isPending}
      />
    </Page>
  );
}
