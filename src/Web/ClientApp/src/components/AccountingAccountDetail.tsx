import type { AccountDto } from '@/features/accounting/types';
import { normalBalanceLabel } from '@/shared/utils/formatters';

interface AccountDetailProps {
  account: AccountDto;
}

export function AccountDetail({ account }: AccountDetailProps) {
  const fields = [
    { label: 'الرمز', value: account.code, dir: 'ltr' as const },
    { label: 'الاسم', value: account.name },
    { label: 'الوصف', value: account.description || '—' },
    { label: 'المجموعة', value: account.accountGroupName },
    { label: 'الحساب الأب', value: account.parentId ?? '—' },
    { label: 'المستوى', value: account.level },
    { label: 'نوع الحساب', value: normalBalanceLabel(account.normalBalance) },
    { label: 'قابل للترحيل', value: account.isPostable ? 'نعم' : 'لا' },
    { label: 'قابل للموازنة', value: account.isReconcilable ? 'نعم' : 'لا' },
    { label: 'العملة', value: account.currencyId ?? '—' },
  ];

  return (
    <div className="bg-[var(--color-surface-container-lowest)] border border-[var(--color-container-border)] rounded-lg p-5">
      <div className="grid grid-cols-[repeat(auto-fill,minmax(14rem,1fr))] gap-5">
        {fields.map((f) => (
          <div key={f.label}>
            <small className="text-[var(--color-on-surface-variant)] font-medium text-xs">{f.label}</small>
            <div className="text-[var(--color-on-surface)] font-medium mt-1" dir={f.dir}>{f.value ?? '—'}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
