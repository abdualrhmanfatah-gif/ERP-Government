// Deposit slip detail — US2 (member management) + US3 (approval)
import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, MoneyDisplay, Badge, Loading, EmptyState, Input, Dialog, Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { DepositSlipsClient, FormType, DepositSlipStatus } from '../../../web-api-client';
import { useEligibleVouchers } from '../hooks/useEligibleVouchers';

const slipClient = new DepositSlipsClient();

const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
};

const formTypeLabels: Record<string, string> = {
  Form47: 'نقدية (47)',
  Form48: 'شيكات (48)',
};

export default function DepositSlipDetailPage() {
  const { id } = useParams<{ id: string }>();
  const slipId = Number(id);
  const queryClient = useQueryClient();

  const { data: slip, isLoading } = useQuery({
    queryKey: ['deposit-slips', slipId],
    queryFn: () => slipClient.depositSlipsGET(slipId),
    enabled: !!slipId,
  });

  const isDraft = slip?.status === DepositSlipStatus.Draft;
  const formType = slip?.formType ?? FormType.Form47;

  // US2: Add voucher
  const [showAddPicker, setShowAddPicker] = useState(false);
  const { eligible } = useEligibleVouchers(
    formType === FormType.Form47 ? 'Form47' : 'Form48',
  );

  const addVoucherMutation = useMutation({
    mutationFn: (voucherId: number) =>
      slipClient.addVoucher(slipId, {
        voucherId,
        rowVersion: slip?.rowVersion ?? '',
      } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deposit-slips', slipId] });
      notify({ type: 'success', title: 'تمت إضافة السند' });
      setShowAddPicker(false);
    },
    onError: () => notify({ type: 'error', title: 'فشلت الإضافة' }),
  });

  // US2: Remove voucher
  const [removeTarget, setRemoveTarget] = useState<number | null>(null);
  const [removeReason, setRemoveReason] = useState('');

  const removeVoucherMutation = useMutation({
    mutationFn: () =>
      slipClient.removeVoucher(slipId, {
        voucherId: removeTarget!,
        reason: removeReason,
        rowVersion: slip?.rowVersion ?? '',
      } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deposit-slips', slipId] });
      notify({ type: 'success', title: 'تمت إزالة السند' });
      setRemoveTarget(null);
      setRemoveReason('');
    },
    onError: () => notify({ type: 'error', title: 'فشلت الإزالة' }),
  });

  // US3: Approve
  const [showApproveDialog, setShowApproveDialog] = useState(false);
  const [approveReason, setApproveReason] = useState('');

  const approveMutation = useMutation({
    mutationFn: () =>
      slipClient.approvePOST4(slipId, {
        reason: approveReason || undefined,
        rowVersion: slip?.rowVersion ?? '',
      } as any),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deposit-slips', slipId] });
      notify({ type: 'success', title: 'تم اعتماد البطاقة' });
      setShowApproveDialog(false);
      setApproveReason('');
    },
    onError: () => notify({ type: 'error', title: 'فشل الاعتماد' }),
  });

  const slipFormType = slip?.formType ?? 'Form47';
  const slipStatus = slip?.status ?? 'Draft';

  return (
    <Page
      title={`بطاقة إيداع ${slip?.slipNumber ?? ''}`}
      loading={isLoading}
      error={!slip ? 'البطاقة غير موجودة' : undefined}
      actions={
        isDraft && slip ? (
          <Button onClick={() => setShowApproveDialog(true)}>
            اعتماد البطاقة
          </Button>
        ) : undefined
      }
    >

      {/* Summary */}
      <div className="grid grid-cols-4 gap-4 text-sm">
        <div>
          <span className="text-muted-foreground">الرقم:</span>{' '}
          <span className="font-medium tabular-nums">{slip.slipNumber}</span>
        </div>
        <div>
          <span className="text-muted-foreground">التاريخ:</span>{' '}
          {new Date(slip.slipDate!).toLocaleDateString('ar-YE')}
        </div>
        <div>
          <span className="text-muted-foreground">النوع:</span>{' '}
          {formTypeLabels[slipFormType]}
        </div>
        <div>
          <span className="text-muted-foreground">الحالة:</span>{' '}
          <Badge variant={isDraft ? 'warning' : 'success'}>
            {statusLabels[slipStatus]}
          </Badge>
        </div>
      </div>

      <div className="text-lg font-semibold">
        الإجمالي: <MoneyDisplay value={slip.totalAmount ?? 0} />
      </div>

      {/* Members */}
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">الأعضاء ({slip.receiptVouchers?.length ?? 0})</h2>
        {isDraft && (
          <Button variant="outline" onClick={() => setShowAddPicker(true)}>
            إضافة سند
          </Button>
        )}
      </div>

      {(slip.receiptVouchers?.length ?? 0) === 0 ? (
        <EmptyState message="لا توجد أعضاء بعد" />
      ) : (
        <div className="border rounded divide-y">
          {slip.receiptVouchers?.map((v: any) => (
            <div key={v.id} className="flex items-center gap-3 px-4 py-3">
              <span className="font-medium tabular-nums">{v.voucherNumber}</span>
              <span className="text-muted-foreground text-sm">{v.receivedFrom}</span>
              <span className="mr-auto">
                <MoneyDisplay value={v.totalAmount ?? 0} />
              </span>
              {isDraft && (
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => setRemoveTarget(v.id)}
                >
                  إزالة
                </Button>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Add voucher picker */}
      <Dialog
        open={showAddPicker}
        onClose={() => setShowAddPicker(false)}
        title="إضافة سند"
        footer={
          <Button variant="outline" className="w-full" onClick={() => setShowAddPicker(false)}>
            إلغاء
          </Button>
        }
      >
        {eligible.length === 0 ? (
          <EmptyState message="لا توجد سندات مؤهلة" />
        ) : (
          <div className="space-y-2">
            {eligible.map((v) => (
              <Button
                key={v.id}
                variant="ghost"
                className="w-full justify-start"
                onClick={() => addVoucherMutation.mutate(v.id ?? 0)}
              >
                <span className="font-medium">{v.voucherNumber}</span>
                <span className="text-muted-foreground text-sm">{v.receivedFrom}</span>
                <span className="ms-auto">
                  <MoneyDisplay value={v.totalAmount ?? 0} />
                </span>
              </Button>
            ))}
          </div>
        )}
      </Dialog>

      {/* Remove voucher dialog */}
      <Dialog
        open={removeTarget != null}
        onClose={() => { setRemoveTarget(null); setRemoveReason(''); }}
        title="إزالة سند"
        footer={
          <>
            <Button
              onClick={() => removeVoucherMutation.mutate()}
              disabled={!removeReason.trim()}
              loading={removeVoucherMutation.isPending}
            >
              تأكيد الإزالة
            </Button>
            <Button variant="outline" onClick={() => { setRemoveTarget(null); setRemoveReason(''); }}>
              إلغاء
            </Button>
          </>
        }
      >
        <Input
          label="السبب (إلزامي)"
          value={removeReason}
          onChange={(e) => setRemoveReason(e.target.value)}
          placeholder="سبب الإزالة..."
        />
      </Dialog>

      {/* Approve dialog — US3 */}
      <Dialog
        open={showApproveDialog}
        onClose={() => { setShowApproveDialog(false); setApproveReason(''); }}
        title="اعتماد بطاقة الإيداع"
        footer={
          <>
            <Button onClick={() => approveMutation.mutate()} loading={approveMutation.isPending}>
              تأكيد الاعتماد
            </Button>
            <Button variant="outline" onClick={() => { setShowApproveDialog(false); setApproveReason(''); }}>
              إلغاء
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted-foreground mb-4">
          سيتم اعتماد البطاقة بشكل نهائي. لا يمكن التراجع.
        </p>
        <Input
          label="سبب الاعتماد (اختياري)"
          value={approveReason}
          onChange={(e) => setApproveReason(e.target.value)}
          placeholder="سبب الاعتماد..."
        />
      </Dialog>
    </Page>
  );
}
