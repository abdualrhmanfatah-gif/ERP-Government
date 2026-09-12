import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Page, Button, Badge, Skeleton, Dialog, FormField, Input, Textarea, ErrorState } from '@/components/ui';
import { useQuotationDetail, useSubmitQuotation, useStartEvaluation, useCompleteEvaluation, useSelectQuotation, useAwardQuotation, useRejectQuotation } from '../hooks/useQuotations';
import { usePartiesList } from '@/features/parties/hooks/useParties';
import { useUnitsList } from '@/features/inventory/units/hooks/useUnits';
import { useItemsList } from '@/features/inventory/items/hooks/useItems';
import { quotationStatusLabels, type QuotationStatus } from '../shared/types';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { QuotationForm } from '../components/QuotationForm';

export default function QuotationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading: detailLoading, error, refetch } = useQuotationDetail(numericId);

  const submitMutation = useSubmitQuotation();
  const startEvalMutation = useStartEvaluation();
  const completeEvalMutation = useCompleteEvaluation();
  const selectMutation = useSelectQuotation();
  const awardMutation = useAwardQuotation();
  const rejectMutation = useRejectQuotation();

  const { data: suppliersData } = usePartiesList({ partyType: 'Supplier' });
  const { data: unitsData } = useUnitsList();
  const { data: itemsData } = useItemsList();

  const [evalDialogOpen, setEvalDialogOpen] = useState(false);
  const [technicalScore, setTechnicalScore] = useState('');
  const [financialScore, setFinancialScore] = useState('');

  const [selectDialogOpen, setSelectDialogOpen] = useState(false);
  const [selectionReason, setSelectionReason] = useState('');

  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');

  const isLoading = detailLoading;

  const supplierOptions = (suppliersData ?? []).map((party) => ({
    value: String(party.id),
    label: party.name,
  }));

  const currencyOptions = [
    { value: 'YER', label: 'ريال يمني (YER)' },
    { value: 'USD', label: 'دولار أمريكي (USD)' },
    { value: 'SAR', label: 'ريال سعودي (SAR)' },
  ];

  const itemOptions = (itemsData?.items ?? []).map((item) => ({
    value: String(item.id),
    label: item.name,
  }));

  const unitOptions = (unitsData ?? []).map((unit) => ({
    value: String(unit.id),
    label: unit.name,
  }));

  if (isLoading) {
    return (
      <Page title="تفاصيل عرض السعر">
        <Skeleton className="h-96" />
      </Page>
    );
  }

  if (error || !detail) {
    return (
      <Page title="تفاصيل عرض السعر">
        <ErrorState message="حدث خطأ أثناء تحميل البيانات" onRetry={refetch} />
      </Page>
    );
  }

  return (
    <Page
      title={`عرض السعر - ${detail.quotationNumber}`}
      maxWidth="full"
      actions={
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate('/procurement/quotations')}>
            القائمة
          </Button>
          {detail.status === 'Draft' && (
            <>
              <Button
                variant="outline"
                onClick={() => navigate(`/procurement/quotations/${numericId}/edit`)}
              >
                تعديل
              </Button>
              <Button
                onClick={() => submitMutation.mutate(numericId, { onError: handleLifecycleError })}
                disabled={submitMutation.isPending}
              >
                تقديم
              </Button>
            </>
          )}
          {detail.status === 'Submitted' && (
            <Button
              onClick={() => startEvalMutation.mutate(numericId, { onError: handleLifecycleError })}
              disabled={startEvalMutation.isPending}
            >
              بدء التقييم
            </Button>
          )}
          {detail.status === 'UnderEvaluation' && (
            <>
              <Button onClick={() => setEvalDialogOpen(true)}>إكمال التقييم</Button>
              <Button variant="destructive" onClick={() => setRejectDialogOpen(true)}>
                رفض
              </Button>
            </>
          )}
          {detail.status === 'Evaluated' && (
            <Button onClick={() => setSelectDialogOpen(true)}>اختيار</Button>
          )}
          {detail.status === 'Selected' && (
            <Button
              onClick={() => awardMutation.mutate(numericId, { onError: handleLifecycleError })}
              disabled={awardMutation.isPending}
            >
              ترسية
            </Button>
          )}
          {detail.status === 'Awarded' && (
            <Button onClick={() => navigate(`/procurement/purchase-orders/create?quotationId=${numericId}`)}>
              إنشاء أمر شراء
            </Button>
          )}
        </div>
      }
    >
      <div className="flex items-center gap-2 mb-4">
        <Badge
          variant={
            detail.status === 'Rejected' || detail.status === 'Expired'
              ? 'destructive'
              : 'default'
          }
        >
          {quotationStatusLabels[detail.status as QuotationStatus]}
        </Badge>
        <span className="text-sm text-[var(--color-on-surface-variant)]" dir="ltr">
          {detail.quotationNumber}
        </span>
      </div>

      <QuotationForm
        initialData={detail}
        readOnly
        onEdit={() => navigate(`/procurement/quotations/${numericId}/edit`)}
        supplierOptions={supplierOptions}
        currencyOptions={currencyOptions}
        itemOptions={itemOptions}
        unitOptions={unitOptions}
      />

      {/* Evaluation Info */}
      {(detail.selectionReason || detail.rejectionReason || detail.technicalScore != null) && (
        <div className="mt-4 p-4 bg-[var(--color-surface-container-low)] rounded-lg">
          <h4 className="text-sm font-medium mb-3">معلومات التقييم</h4>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm">
            {detail.technicalScore != null && (
              <div>
                <span className="text-xs text-[var(--color-on-surface-variant)]">الدرجة التقنية</span>
                <p className="font-medium">{detail.technicalScore}</p>
              </div>
            )}
            {detail.financialScore != null && (
              <div>
                <span className="text-xs text-[var(--color-on-surface-variant)]">الدرجة المالية</span>
                <p className="font-medium">{detail.financialScore}</p>
              </div>
            )}
            {detail.selectionReason && (
              <div>
                <span className="text-xs text-[var(--color-on-surface-variant)]">سبب الاختيار</span>
                <p className="font-medium">{detail.selectionReason}</p>
              </div>
            )}
            {detail.rejectionReason && (
              <div>
                <span className="text-xs text-[var(--color-on-surface-variant)]">سبب الرفض</span>
                <p className="font-medium text-[var(--color-error)]">{detail.rejectionReason}</p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Dialogs */}
      <Dialog
        open={evalDialogOpen}
        onClose={() => setEvalDialogOpen(false)}
        title="إكمال التقييم"
        footer={
          <>
            <Button variant="outline" onClick={() => setEvalDialogOpen(false)}>
              إلغاء
            </Button>
            <Button
              onClick={() => {
                completeEvalMutation.mutate(
                  {
                    id: numericId,
                    technicalScore: Number(technicalScore),
                    financialScore: Number(financialScore),
                  },
                  { onSuccess: () => setEvalDialogOpen(false), onError: handleLifecycleError }
                );
              }}
              disabled={completeEvalMutation.isPending || !technicalScore || !financialScore}
            >
              {completeEvalMutation.isPending ? 'جاري الحفظ...' : 'تأكيد'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="الدرجة التقنية" required>
            <Input
              type="number"
              step="0.01"
              value={technicalScore}
              onChange={(e) => setTechnicalScore(e.target.value)}
            />
          </FormField>
          <FormField label="الدرجة المالية" required>
            <Input
              type="number"
              step="0.01"
              value={financialScore}
              onChange={(e) => setFinancialScore(e.target.value)}
            />
          </FormField>
        </div>
      </Dialog>

      <Dialog
        open={selectDialogOpen}
        onClose={() => setSelectDialogOpen(false)}
        title="اختيار العرض"
        footer={
          <>
            <Button variant="outline" onClick={() => setSelectDialogOpen(false)}>
              إلغاء
            </Button>
            <Button
              onClick={() => {
                selectMutation.mutate(
                  { id: numericId, selectionReason },
                  { onSuccess: () => setSelectDialogOpen(false), onError: handleLifecycleError }
                );
              }}
              disabled={selectMutation.isPending || !selectionReason.trim()}
            >
              {selectMutation.isPending ? 'جاري الحفظ...' : 'تأكيد'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="سبب الاختيار" required>
            <Textarea
              rows={3}
              value={selectionReason}
              onChange={(e) => setSelectionReason(e.target.value)}
            />
          </FormField>
        </div>
      </Dialog>

      <Dialog
        open={rejectDialogOpen}
        onClose={() => setRejectDialogOpen(false)}
        title="رفض العرض"
        footer={
          <>
            <Button variant="outline" onClick={() => setRejectDialogOpen(false)}>
              إلغاء
            </Button>
            <Button
              variant="destructive"
              onClick={() => {
                rejectMutation.mutate(
                  { id: numericId, rejectionReason },
                  { onSuccess: () => setRejectDialogOpen(false), onError: handleLifecycleError }
                );
              }}
              disabled={rejectMutation.isPending || !rejectionReason.trim()}
            >
              {rejectMutation.isPending ? 'جاري الحفظ...' : 'رفض'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="سبب الرفض" required>
            <Textarea
              rows={3}
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
            />
          </FormField>
        </div>
      </Dialog>
    </Page>
  );
}
