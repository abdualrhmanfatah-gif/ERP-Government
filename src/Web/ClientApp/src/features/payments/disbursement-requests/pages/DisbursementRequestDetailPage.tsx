import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useDisbursementRequestDetail, useSubmitDisbursementRequest, useApproveDisbursementRequest, useRejectDisbursementRequest, useCancelDisbursementRequest, useGetAccrualEntry, useCreateAccrualEntry } from '../hooks/useDisbursementRequests';
import { approvalDecisionLabels, getFinalApprovedAmount } from '../shared/types';
import { useUserDetail } from '@/features/security/users/hooks';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useCurrenciesList } from '@/features/accounting/hooks/useCurrenciesList';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';
import { DisbursementRequestForm } from '@/components/DisbursementRequestForm';
import { Page, Button, Card, Dialog, Input, Textarea, MoneyDisplay, ErrorState, Badge, Combobox } from '@/components/ui';
import { MetaItem } from '@/components/MetaItem';
import { ArrowRight, Check } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { getAuthUser } from '@/shared/utils/auth-token';

export default function DisbursementRequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const requestId = Number(id);

  const { data: request, isLoading, isError, refetch } = useDisbursementRequestDetail(requestId);
  const { data: currentUser } = useUserDetail(getAuthUser()?.userId ?? 0);
  const submitMutation = useSubmitDisbursementRequest();
  const approveMutation = useApproveDisbursementRequest();
  const rejectMutation = useRejectDisbursementRequest();
  const cancelMutation = useCancelDisbursementRequest();
  const { data: accrualEntry, isLoading: isLoadingAccrual } = useGetAccrualEntry(requestId);
  const createAccrualMutation = useCreateAccrualEntry();
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: currencies = [] } = useCurrenciesList();

  const [dialogState, setDialogState] = useState<'approve' | 'reject' | 'cancel' | 'accrual' | null>(null);
  const [approveAmount, setApproveAmount] = useState<number>(0);
  const [approveAuthorityName, setApproveAuthorityName] = useState('');
  const [approveAuthorityCapacity, setApproveAuthorityCapacity] = useState('');
  const [rejectReason, setRejectReason] = useState('');
  const [cancelReason, setCancelReason] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [formState, setFormState] = useState({ canSave: false, isSaving: false });

  // Accrual entry form state
  const [accrualExpenseAccountId, setAccrualExpenseAccountId] = useState<number>(0);
  const [accrualLiabilityAccountId, setAccrualLiabilityAccountId] = useState<number>(0);
  const [accrualAmount, setAccrualAmount] = useState<number>(0);
  const [accrualCurrencyId, setAccrualCurrencyId] = useState<number>(0);
  const [accrualNarration, setAccrualNarration] = useState('');

  function toBase64(rowVersion: string  | string): string {
    if (typeof rowVersion === 'string') return rowVersion;
    const bytes = new Uint8Array(rowVersion);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i]);
    return btoa(binary);
  }

  useEffect(() => {
    if (currentUser) {
      setApproveAuthorityName(currentUser.login ?? '');
      setApproveAuthorityCapacity(currentUser.role?.name ?? '');
    }
  }, [currentUser]);

  if (isLoading) return <Page title="طلب صرف" loading />;
  if (isError || !request) return <Page title="طلب صرف"><ErrorState message="فشل تحميل تفاصيل الطلب" onRetry={() => refetch()} /></Page>;

  const status = request.status as string;
  const approvals = (request as any).approvals ?? [];
  const validApprovals = approvals.filter((a: any) => a.decision === 'Approved');
  const firstApproval = validApprovals[0] ?? null;
  const secondApproval = validApprovals[1] ?? null;
  const currentStep = validApprovals.length + 1;
  const isFinalApproved = validApprovals.length >= 2;
  const approvalsStarted = validApprovals.length > 0;

  const canSubmit = status === 'Draft';
  const canEdit = status === 'Draft' || status === 'PendingApproval';
  const canApprove = status === 'PendingApproval' && currentStep <= 2;
  const canRetryFinalize = status === 'PendingApproval' && isFinalApproved && !(request as any).paymentOrderId;
  const canReject = status === 'PendingApproval';
  const canCancel = status === 'Draft' || status === 'PendingApproval';
  const canCreateAccrual = status === 'Approved' && !accrualEntry && !isLoadingAccrual;

  const handleSubmit = async () => {
    try {
      await submitMutation.mutateAsync(requestId);
      notify({ type: 'success', title: 'تم تقديم طلب الصرف' });
      refetch();
    } catch {
      notify({ type: 'error', title: 'فشل تقديم طلب الصرف' });
    }
    setDialogState(null);
  };

  const handleApprove = async () => {
    if (!approveAuthorityName.trim() || !approveAuthorityCapacity.trim()) {
      notify({ type: 'error', title: 'اسم جهة الأمر وصفته مطلوبان' });
      return;
    }
    try {
      await approveMutation.mutateAsync({
        id: requestId,
        cmd: {
          approvedAmount: approveAmount,
          issuingAuthorityName: approveAuthorityName.trim(),
          issuingAuthorityCapacity: approveAuthorityCapacity.trim(),
          rowVersion: toBase64((request as any).rowVersion ?? []),
        },
      });
      notify({ type: 'success', title: currentStep === 2 ? 'تم التوقيع الثاني والاعتماد النهائي' : 'تم التوقيع الأول' });
      refetch();
    } catch {
      notify({ type: 'error', title: 'فشل التوقيع على طلب الصرف' });
    }
    setDialogState(null);
    setApproveAmount(0);
    setApproveAuthorityName('');
    setApproveAuthorityCapacity('');
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      notify({ type: 'error', title: 'سبب الرفض مطلوب' });
      return;
    }
    try {
      await rejectMutation.mutateAsync({
        id: requestId,
        cmd: { reason: rejectReason.trim(), rowVersion: toBase64((request as any).rowVersion ?? []) },
      });
      notify({ type: 'success', title: 'تم رفض طلب الصرف' });
      refetch();
    } catch {
      notify({ type: 'error', title: 'فشل رفض طلب الصرف' });
    }
    setDialogState(null);
    setRejectReason('');
  };

  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      notify({ type: 'error', title: 'سبب الإلغاء مطلوب' });
      return;
    }
    try {
      await cancelMutation.mutateAsync({
        id: requestId,
        cmd: { reason: cancelReason.trim(), rowVersion: toBase64((request as any).rowVersion ?? []) },
      });
      notify({ type: 'success', title: 'تم إلغاء طلب الصرف' });
      refetch();
    } catch {
      notify({ type: 'error', title: 'فشل إلغاء طلب الصرف' });
    }
    setDialogState(null);
    setCancelReason('');
  };

  const handleCreateAccrual = async () => {
    if (!accrualExpenseAccountId || !accrualLiabilityAccountId || !accrualAmount || !accrualCurrencyId) {
      notify({ type: 'error', title: 'جميع الحقول مطلوبة' });
      return;
    }
    if (accrualExpenseAccountId === accrualLiabilityAccountId) {
      notify({ type: 'error', title: 'يجب أن يكون الحسابان مختلفين' });
      return;
    }
    try {
      await createAccrualMutation.mutateAsync({
        id: requestId,
        cmd: {
          expenseAccountId: accrualExpenseAccountId,
          liabilityAccountId: accrualLiabilityAccountId,
          amount: accrualAmount,
          currencyId: accrualCurrencyId,
          narration: accrualNarration || undefined,
        },
      });
      notify({ type: 'success', title: 'تم إنشاء قيد الاستحقاق' });
      refetch();
    } catch {
      notify({ type: 'error', title: 'فشل إنشاء قيد الاستحقاق' });
    }
    setDialogState(null);
    resetAccrualForm();
  };

  const resetAccrualForm = () => {
    setAccrualExpenseAccountId(0);
    setAccrualLiabilityAccountId(0);
    setAccrualAmount(0);
    setAccrualCurrencyId(0);
    setAccrualNarration('');
  };

  const headerActions = isEditing ? (
    <div className="flex items-center gap-2 flex-wrap">
      <Button
        type="submit"
        form="disbursement-request-detail-form"
        variant="primary"
        size="sm"
        disabled={!formState.canSave || formState.isSaving}
        loading={formState.isSaving}
      >
        حفظ التعديلات
      </Button>
      <Button variant="ghost" size="sm" onClick={() => setIsEditing(false)} disabled={formState.isSaving}>
        إلغاء
      </Button>
    </div>
  ) : (
    <div className="flex items-center gap-2 flex-wrap">
      {canEdit && (
        <Button variant="secondary" size="sm" onClick={() => setIsEditing(true)}>
          تعديل
        </Button>
      )}
      {canSubmit && (
        <Button variant="primary" size="sm" onClick={handleSubmit} disabled={submitMutation.isPending} loading={submitMutation.isPending}>
          تقديم
        </Button>
      )}
      {canApprove && (
        <Button
          variant="primary"
          size="sm"
          onClick={() => {
            setApproveAmount(firstApproval?.approvedAmount ?? request.requestedAmount);
            setApproveAuthorityName(firstApproval?.issuingAuthorityName ?? currentUser?.login ?? '');
            setApproveAuthorityCapacity(firstApproval?.issuingAuthorityCapacity ?? currentUser?.role?.name ?? '');
            setDialogState('approve');
          }}
        >
          {currentStep === 1 ? 'التوقيع الأول' : 'التوقيع الثاني والاعتماد النهائي'}
        </Button>
      )}
      {canRetryFinalize && (
        <Button
          variant="primary"
          size="sm"
          onClick={() => {
            setApproveAmount(firstApproval?.approvedAmount ?? request.requestedAmount);
            setApproveAuthorityName(firstApproval?.issuingAuthorityName ?? currentUser?.login ?? '');
            setApproveAuthorityCapacity(firstApproval?.issuingAuthorityCapacity ?? currentUser?.role?.name ?? '');
            setDialogState('approve');
          }}
        >
          إعادة التوقيع
        </Button>
      )}
      {canReject && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('reject')}>
          رفض
        </Button>
      )}
      {canCancel && (
        <Button variant="destructive" size="sm" onClick={() => setDialogState('cancel')}>
          إلغاء
        </Button>
      )}
      {canCreateAccrual && (
        <Button variant="primary" size="sm" onClick={() => {
          setAccrualAmount(request.requestedAmount);
          setAccrualCurrencyId(request.currencyId);
          setDialogState('accrual');
        }}>
          إنشاء قيد الاستحقاق
        </Button>
      )}
      <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
        <ArrowRight size={18} />
      </Button>
    </div>
  );

  return (
    <Page
      title={`طلب صرف — ${request.requestNumber}`}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <PaymentsStatusBadge status={status} variant="request" />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="رقم الطلب" value={request.requestNumber} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={request.requestDate} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="المقدم" value={request.requestedByName} />
        </div>
      }
    >
      {/* التخطيط الرئيسي: عمودان على سطح المكتب */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* العمود الأيسر: النموذج + تفاصيل التوقيعات */}
        <div className="lg:col-span-8 space-y-6">
          <DisbursementRequestForm
            mode="detail"
            initialData={request}
            editing={isEditing}
            lockedCoreFields={approvalsStarted}
            onToggleEditing={setIsEditing}
            onStateChange={setFormState}
            onSaved={() => { refetch(); setIsEditing(false); }}
          />

          {/* تفاصيل التوقيعات */}
          {approvals.length > 0 && (
            <Card>
              <h2 className="text-sm font-semibold mb-4">تفاصيل التوقيعات</h2>
              <div className="space-y-3">
                {approvals.map((a: any, i: number) => (
                  <div key={i} className="p-3 rounded-md bg-[var(--color-surface-container-low)] text-sm">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="text-xs text-[var(--color-on-surface-variant)]">التوقيع {a.step}</span>
                      <Badge variant={a.decision === 'Approved' ? 'success' : 'danger'}>
                        {approvalDecisionLabels[a.decision] ?? a.decision}
                      </Badge>
                    </div>
                    <div className="grid grid-cols-2 gap-2 text-xs">
                      <div><span className="text-[var(--color-on-surface-variant)]">الموقّع: </span>{a.approverName}</div>
                      <div><span className="text-[var(--color-on-surface-variant)]">الدور: </span>{a.role}</div>
                      <div><span className="text-[var(--color-on-surface-variant)]">المبلغ: </span><MoneyDisplay value={a.approvedAmount ?? 0} /></div>
                      {a.decisionAt && (
                        <div><span className="text-[var(--color-on-surface-variant)]">الوقت: </span>{new Date(a.decisionAt).toLocaleDateString('ar-YE')}</div>
                      )}
                      {a.issuingAuthorityName && (
                        <div className="col-span-2"><span className="text-[var(--color-on-surface-variant)]">جهة الإصدار: </span>{a.issuingAuthorityName} — {a.issuingAuthorityCapacity}</div>
                      )}
                      {a.reason && (
                        <div className="col-span-2"><span className="text-[var(--color-on-surface-variant)]">ملاحظات: </span>{a.reason}</div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </div>

        {/* العمود الأيمن: المرفقات + تقدم الاعتماد */}
        <div className="lg:col-span-4 space-y-6">
          <AttachmentsPanel documentType="DisbursementRequest" documentId={requestId} showGate={canSubmit || canApprove} />

          <Card>
            <h2 className="text-sm font-semibold mb-4">تقدم الاعتماد</h2>
            
            {/* خط التقدم */}
            <div className="flex items-center gap-2 mb-4">
              {[1, 2].map(step => {
                const isDone = validApprovals.some((a: any) => a.step === step && a.decision === 'Approved');
                const isCurrentStep = step === currentStep && status === 'PendingApproval' && !isDone;
                return (
                  <div key={step} className="flex items-center gap-2">
                    <span
                      className={`inline-flex items-center justify-center w-8 h-8 rounded-full text-sm font-bold transition-all ${
                        isDone
                          ? 'bg-[var(--color-primary)] text-[var(--color-on-primary)]'
                          : isCurrentStep
                            ? 'bg-[var(--color-secondary)] text-[var(--color-on-secondary)] ring-2 ring-[var(--color-secondary)] ring-offset-2'
                            : 'bg-[var(--color-surface-container)] text-[var(--color-on-surface-variant)] border border-[var(--color-outline)]'
                      }`}
                    >
                      {isDone ? <Check size={16} /> : step}
                    </span>
                    {step === 1 && (
                      <span className="text-[var(--color-outline)]" aria-hidden="true">—</span>
                    )}
                  </div>
                );
              })}
            </div>

            {isFinalApproved && secondApproval ? (
              <div className="space-y-2 text-sm">
                <div>
                  <span className="text-[var(--color-on-surface-variant)]">المبلغ المعتمد النهائي</span>
                  <p className="font-semibold">
                    {getFinalApprovedAmount(approvals) != null ? (
                      <MoneyDisplay value={getFinalApprovedAmount(approvals)!} />
                    ) : (
                      <span>—</span>
                    )}
                  </p>
                </div>
                <div>
                  <span className="text-[var(--color-on-surface-variant)]">جهة الإصدار</span>
                  <p className="font-semibold">{secondApproval.issuingAuthorityName ?? '—'}</p>
                </div>
                <div>
                  <span className="text-[var(--color-on-surface-variant)]">الصفة</span>
                  <p className="font-semibold">{secondApproval.issuingAuthorityCapacity ?? '—'}</p>
                </div>
                <div>
                  <span className="text-[var(--color-on-surface-variant)]">وقت الاعتماد النهائي</span>
                  <p className="font-semibold">
                    {secondApproval.decisionAt ? new Date(secondApproval.decisionAt).toLocaleDateString('ar-YE') : '—'}
                  </p>
                </div>
              </div>
            ) : (
              <p className="text-sm text-[var(--color-on-surface-variant)]">لم يعتمد بعد</p>
            )}
          </Card>
        </div>
      </div>

      {/* المعلومات الداعمة أسفل */}
      <div className="mt-6 space-y-6">
        {/* القرار السلبي */}
        {status === 'Rejected' && (
          <Card>
            <h2 className="text-sm font-semibold mb-2">سبب الرفض</h2>
            <p className="text-sm">{approvals.find((a: any) => a.decision === 'Rejected')?.reason ?? 'غير متوفر'}</p>
          </Card>
        )}
        {(status === 'Cancelled' || status === 'Invalidated') && (
          <Card>
            <h2 className="text-sm font-semibold mb-2">تفاصيل الإلغاء</h2>
            <p className="text-sm">{approvals.find((a: any) => a.decision === 'Cancelled')?.reason ?? 'غير متوفر'}</p>
          </Card>
        )}

        {/* السجلات المشتقة:أمر الصرف + قيد الاستحقاق */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <h2 className="text-sm font-semibold mb-4">أمر الصرف الناتج</h2>
            {(request as any).paymentOrderId ? (
              <Link
                to={`/payments/payment-orders/${(request as any).paymentOrderId}`}
                className="text-[var(--color-primary)] hover:underline font-mono"
              >
                {(request as any).paymentOrderNumber ?? (request as any).paymentOrderId}
              </Link>
            ) : (
              <p className="text-sm text-[var(--color-on-surface-variant)]">لم يتم إنشاء أمر بعد</p>
            )}
          </Card>

          <Card>
            <h2 className="text-sm font-semibold mb-4">قيد الاستحقاق</h2>
            {isLoadingAccrual ? (
              <p className="text-sm text-[var(--color-on-surface-variant)]">جارٍ التحقق...</p>
            ) : accrualEntry ? (
              <div className="space-y-2">
                <div className="flex items-center gap-2">
                  <Badge variant="success">موجود</Badge>
                  <Link
                    to={`/accounting/journal-entries/${accrualEntry.id}`}
                    className="text-[var(--color-primary)] hover:underline font-mono"
                  >
                    {accrualEntry.entryNumber}
                  </Link>
                </div>
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div><span className="text-[var(--color-on-surface-variant)]">الحساب المدين: </span>{accrualEntry.expenseAccountCode} - {accrualEntry.expenseAccountName}</div>
                  <div><span className="text-[var(--color-on-surface-variant)]">الحساب الدائن: </span>{accrualEntry.liabilityAccountCode} - {accrualEntry.liabilityAccountName}</div>
                  <div><span className="text-[var(--color-on-surface-variant)]">المبلغ: </span><MoneyDisplay value={accrualEntry.amount} /></div>
                  <div><span className="text-[var(--color-on-surface-variant)]">التاريخ: </span>{accrualEntry.documentDate}</div>
                </div>
              </div>
            ) : (
              <p className="text-sm text-[var(--color-on-surface-variant)]">لم يتم إنشاء قيد استحقاق بعد</p>
            )}
          </Card>
        </div>

        {/* سجل الحالة */}
        <Card>
          <div className="p-6"><StatusLogPanel documentType="DisbursementRequest" documentId={requestId} /></div>
        </Card>
      </div>

      {/* نافذة الموافقة/التوقيع */}
      <Dialog
        open={dialogState === 'approve'}
        onClose={() => setDialogState(null)}
        title={currentStep === 1 ? 'التوقيع الأول على طلب الصرف' : 'التوقيع الثاني والاعتماد النهائي'}
        footer={
          <>
            <Button variant="outline" onClick={() => setDialogState(null)}>إلغاء</Button>
            <Button onClick={handleApprove} disabled={approveMutation.isPending}>
              {approveMutation.isPending ? 'جارٍ التنفيذ...' : currentStep === 1 ? 'تأكيد التوقيع الأول' : 'تأكيد التوقيع الثاني والاعتماد النهائي'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="p-3 rounded-md bg-[var(--color-surface-container-low)] text-sm">
            <div>رقم الطلب: <span className="font-mono">{request.requestNumber}</span></div>
            <div>المستفيد: {request.beneficiaryName}</div>
            <div>المبلغ المطلوب: <MoneyDisplay value={request.requestedAmount} /></div>
          </div>

          {currentStep === 2 && firstApproval && (
            <div className="p-3 rounded-md bg-[var(--color-primary-container)] text-sm">
              <div>مبلغ التوقيع الأول: <MoneyDisplay value={firstApproval.approvedAmount ?? 0} /></div>
              <div className="text-xs text-[var(--color-on-surface-variant)] mt-1">يجب تأكيد نفس المبلغ في التوقيع الثاني</div>
            </div>
          )}

          <div>
            <Input
              label="المبلغ المعتمد"
              type="number"
              step="0.01"
              value={approveAmount || ''}
              onChange={(e) => setApproveAmount(Number(e.target.value))}
              max={request.requestedAmount}
            />
            <p className="text-xs text-[var(--color-on-surface-variant)] mt-1">
              المبلغ الأصلي: <MoneyDisplay value={request.requestedAmount} />
            </p>
          </div>

          <div>
            <Input
              label="اسم جهة الأمر"
              value={approveAuthorityName}
              onChange={(e) => setApproveAuthorityName(e.target.value)}
              placeholder="المدير العام أو المدير المالي"
            />
          </div>

          <div>
            <Input
              label="صفة جهة الأمر"
              value={approveAuthorityCapacity}
              onChange={(e) => setApproveAuthorityCapacity(e.target.value)}
              placeholder="المدير العام / المدير المالي"
            />
          </div>
        </div>
      </Dialog>

      {/* نافذة الرفض */}
      <Dialog
        open={dialogState === 'reject'}
        onClose={() => setDialogState(null)}
        title="رفض طلب الصرف"
        footer={
          <>
            <Button variant="outline" onClick={() => setDialogState(null)}>إلغاء</Button>
            <Button variant="destructive" onClick={handleReject} disabled={rejectMutation.isPending}>
              {rejectMutation.isPending ? 'جارٍ الرفض...' : 'تأكيد الرفض'}
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الرفض"
          value={rejectReason}
          onChange={(e) => setRejectReason(e.target.value)}
          rows={3}
          placeholder="اذكر سبب الرفض..."
        />
      </Dialog>

      {/* نافذة الإلغاء */}
      <Dialog
        open={dialogState === 'cancel'}
        onClose={() => setDialogState(null)}
        title="إلغاء طلب الصرف"
        footer={
          <>
            <Button variant="outline" onClick={() => setDialogState(null)}>إلغاء</Button>
            <Button variant="destructive" onClick={handleCancel} disabled={cancelMutation.isPending}>
              {cancelMutation.isPending ? 'جارٍ الإلغاء...' : 'تأكيد الإلغاء'}
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الإلغاء"
          value={cancelReason}
          onChange={(e) => setCancelReason(e.target.value)}
          rows={3}
          placeholder="اذكر سبب الإلغاء..."
        />
      </Dialog>

      {/* نافذة إنشاء قيد الاستحقاق */}
      <Dialog
        open={dialogState === 'accrual'}
        onClose={() => { setDialogState(null); resetAccrualForm(); }}
        title="إنشاء قيد الاستحقاق"
        footer={
          <>
            <Button variant="outline" onClick={() => { setDialogState(null); resetAccrualForm(); }}>إلغاء</Button>
            <Button onClick={handleCreateAccrual} disabled={createAccrualMutation.isPending}>
              {createAccrualMutation.isPending ? 'جارٍ الإنشاء...' : 'تأكيد الإنشاء'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <div className="p-3 rounded-md bg-[var(--color-surface-container-low)] text-sm">
            <div>رقم الطلب: <span className="font-mono">{request.requestNumber}</span></div>
            <div>المستفيد: {request.beneficiaryName}</div>
            <div>المبلغ المطلوب: <MoneyDisplay value={request.requestedAmount} /></div>
          </div>

          <div>
            <Combobox
              label="حساب المصروف"
              value={accrualExpenseAccountId ? String(accrualExpenseAccountId) : ''}
              onChange={(val) => setAccrualExpenseAccountId(val ? Number(val) : 0)}
              options={accounts.map((a: any) => ({ value: String(a.id), label: `${a.code} - ${a.name}` }))}
              placeholder="اختر حساب المصروف..."
              searchPlaceholder="بحث بالرمز أو الاسم..."
              emptyMessage="لا توجد حسابات"
            />
          </div>

          <div>
            <Combobox
              label="حساب الخصم (الدائن)"
              value={accrualLiabilityAccountId ? String(accrualLiabilityAccountId) : ''}
              onChange={(val) => setAccrualLiabilityAccountId(val ? Number(val) : 0)}
              options={accounts.map((a: any) => ({ value: String(a.id), label: `${a.code} - ${a.name}` }))}
              placeholder="اختر حساب الخصم..."
              searchPlaceholder="بحث بالرمز أو الاسم..."
              emptyMessage="لا توجد حسابات"
            />
          </div>

          <div>
            <Input
              label="المبلغ"
              type="number"
              step="0.01"
              value={accrualAmount || ''}
              onChange={(e) => setAccrualAmount(Number(e.target.value))}
              max={request.requestedAmount}
            />
            <p className="text-xs text-[var(--color-on-surface-variant)] mt-1">
              المبلغ الأصلي: <MoneyDisplay value={request.requestedAmount} />
            </p>
          </div>

          <div>
            <Combobox
              label="العملة"
              value={accrualCurrencyId ? String(accrualCurrencyId) : ''}
              onChange={(val) => setAccrualCurrencyId(val ? Number(val) : 0)}
              options={(currencies as any[]).map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` }))}
              placeholder="اختر العملة..."
              searchPlaceholder="بحث بالرمز أو الاسم..."
              emptyMessage="لا توجد عملات"
            />
          </div>

          <div>
            <Textarea
              label="البيان"
              value={accrualNarration}
              onChange={(e) => setAccrualNarration(e.target.value)}
              rows={2}
              placeholder="اختياري..."
            />
          </div>
        </div>
      </Dialog>
    </Page>
  );
}
