import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { FilterBar, FilterSelect } from '@/components/ui';
import { JournalGrid } from '@/components/AccountingJournalGrid';
import { useJournalsList } from '../../hooks/useJournalsList';
import { JournalType } from '../../../../web-api-client';

const journalTypeOptions = [
  { value: '', label: 'الكل' },
  { value: JournalType.General, label: 'عامة' },
  { value: JournalType.Purchase, label: 'مشتريات' },
  { value: JournalType.Sale, label: 'مبيعات' },
  { value: JournalType.Cash, label: 'نقدية' },
  { value: JournalType.Bank, label: 'بنكية' },
  { value: JournalType.Adjustment, label: 'تسوية' },
  { value: JournalType.Closing, label: 'إقفال' },
];

const activeOptions = [
  { value: '', label: 'الكل' },
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

export function JournalsListPage() {
  const navigate = useNavigate();
  const [type, setType] = useState<string>('');
  const [isActive, setIsActive] = useState<string>('');

  const { data: journals = [], isLoading, error, refetch } = useJournalsList({
    type: type ? (type as JournalType) : undefined,
    isActive: isActive === '' ? undefined : isActive === 'true',
  });

  const hasFilters = !!type || !!isActive;

  const clearAll = () => {
    setType('');
    setIsActive('');
  };

  return (
    <div>
      <PageHeader
        title="دفاتر اليومية"
        description="إدارة دفاتر اليومية المحاسبية"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/accounting/journals/create')}>
            إنشاء دفتر
          </Button>
        }
      />
      <FilterBar hasFilters={hasFilters} onClear={clearAll}>
        <FilterSelect
          label="النوع"
          value={type}
          onChange={setType}
          options={journalTypeOptions}
        />
        <FilterSelect
          label="الحالة"
          value={isActive}
          onChange={setIsActive}
          options={activeOptions}
        />
      </FilterBar>
      <div className="mt-4">
        <JournalGrid
          data={journals}
          loading={isLoading}
          error={error ? 'فشل تحميل البيانات' : undefined}
          onRetry={() => refetch()}
        />
      </div>
    </div>
  );
}
