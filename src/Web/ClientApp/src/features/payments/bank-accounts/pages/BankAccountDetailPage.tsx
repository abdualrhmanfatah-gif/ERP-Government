import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBankAccountDetail, useActivateBankAccount, useDeactivateBankAccount } from '../hooks/useBankAccounts';
import { bankAccountStatusLabels } from '../shared/types';
import { Page, Button, Card, ConfirmDialog, MoneyDisplay, Badge } from '@/components/ui';

export default function BankAccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = Number(id);

  const { data: account, isLoading } = useBankAccountDetail(accountId);
  const activateMutation = useActivateBankAccount();
  const deactivateMutation = useDeactivateBankAccount();
  const [showConfirm, setShowConfirm] = useState(false);

  const handleToggleActive = async () => {
    if (!account) return;
    try {
      if (account.isActive) {
        await deactivateMutation.mutateAsync({ id: accountId, cmd: {} });
      } else {
        await activateMutation.mutateAsync({ id: accountId, cmd: {} });
      }
      navigate(0);
    } catch {
      // error handled by mutation
    } finally {
      setShowConfirm(false);
    }
  };

  if (isLoading) return <Page title="جارٍ التحميل..." loading />;
  if (!account) return <Page title="لم يتم العثور على الحساب البنكي" />;

  const isActive = account.isActive ?? false;

  return (
    <Page title={`حساب بنكي — ${account.name}`}>
      <div className="space-y-6">
        <div className="flex gap-3 items-center">
          <Badge variant={isActive ? 'success' : 'default'}>
            {isActive ? bankAccountStatusLabels.active : bankAccountStatusLabels.inactive}
          </Badge>
          {account.isDefault && <Badge variant="primary">افتراضي</Badge>}
        </div>

        <Card>
          <h2 className="text-sm font-semibold mb-4">البيانات الأساسية</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">الاسم:</span> {account.name}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">البنك:</span> {account.bankName}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">رقم الحساب:</span> <span className="font-mono">{account.accountNumber}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">IBAN:</span> <span className="font-mono">{account.iban ?? '—'}</span></div>
          </div>
        </Card>

        <Card>
          <h2 className="text-sm font-semibold mb-4">الفرع</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">اسم الفرع:</span> {account.branchName ?? '—'}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">كود الفرع:</span> <span className="font-mono">{account.branchCode ?? '—'}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">SWIFT:</span> <span className="font-mono">{account.swiftCode ?? '—'}</span></div>
          </div>
        </Card>

        <Card>
          <h2 className="text-sm font-semibold mb-4">البيانات المالية</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">العملة:</span> {account.currencyId}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">الرصيد الحالي:</span> <MoneyDisplay value={account.currentBalance ?? 0} /></div>
            <div><span className="text-[var(--color-on-surface-variant)]">الرصيد الافتتاحي:</span> <MoneyDisplay value={account.openingBalance ?? 0} /></div>
          </div>
        </Card>

        <Card>
          <h2 className="text-sm font-semibold mb-4">ضوابط</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">الحد اليومي:</span> <MoneyDisplay value={account.maxDailyLimit ?? 0} /></div>
            <div><span className="text-[var(--color-on-surface-variant)]">حد المعاملة:</span> <MoneyDisplay value={account.maxTransactionLimit ?? 0} /></div>
            <div><span className="text-[var(--color-on-surface-variant)]">تتطلب موافقة مزدوجة:</span> {account.requiresDualApproval ? 'نعم' : 'لا'}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">آخر تسوية:</span> {account.lastReconciliationDate ? new Date(account.lastReconciliationDate).toLocaleDateString('ar-YE') : '—'}</div>
          </div>
        </Card>

        <Card>
          <h2 className="text-sm font-semibold mb-4">إجراءات</h2>
          <Button variant={isActive ? 'destructive' : 'default'} size="sm" onClick={() => setShowConfirm(true)} disabled={activateMutation.isPending || deactivateMutation.isPending}>
            {isActive ? 'تعطيل' : 'تنشيط'}
          </Button>
        </Card>
      </div>

      <ConfirmDialog
        open={showConfirm}
        onClose={() => setShowConfirm(false)}
        onConfirm={handleToggleActive}
        title={isActive ? 'تعطيل الحساب البنكي' : 'تنشيط الحساب البنكي'}
        message={isActive ? 'هل أنت متأكد من تعطيل هذا الحساب البنكي؟ لا يمكن استخدامه في المعاملات الجديدة بعد التعطيل.' : 'هل أنت متأكد من تنشيط هذا الحساب البنكي؟'}
        confirmLabel="تأكيد"
        destructive={isActive}
        loading={activateMutation.isPending || deactivateMutation.isPending}
      />
    </Page>
  );
}
