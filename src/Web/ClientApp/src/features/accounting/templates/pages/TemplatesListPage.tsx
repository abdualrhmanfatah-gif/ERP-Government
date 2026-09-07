import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { FilterBar, FilterSelect } from '@/components/ui';
import { TemplateGrid } from '@/components/AccountingTemplateGrid';
import { useTemplatesList } from '../../hooks/useTemplatesList';
import { useJournalsList } from '../../hooks/useJournalsList';
import { JournalEntryTemplateType } from '../../../../web-api-client';

const templateTypeOptions = [
  { value: '', label: 'الكل' },
  { value: JournalEntryTemplateType.Standard, label: 'قياسية' },
  { value: JournalEntryTemplateType.Recurring, label: 'دورية' },
  { value: JournalEntryTemplateType.Adjustment, label: 'تسوية' },
];

const activeOptions = [
  { value: '', label: 'الكل' },
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

export function TemplatesListPage() {
  const navigate = useNavigate();
  const [templateType, setTemplateType] = useState<string>('');
  const [isActive, setIsActive] = useState<string>('');
  const [journalId, setJournalId] = useState<string>('');

  const { data: journals = [] } = useJournalsList();
  const { data: templates = [], isLoading, error, refetch } = useTemplatesList({
    templateType: templateType ? (templateType as JournalEntryTemplateType) : undefined,
    isActive: isActive === '' ? undefined : isActive === 'true',
    journalId: journalId ? Number(journalId) : undefined,
  });

  const hasFilters = !!templateType || !!isActive || !!journalId;

  const clearAll = () => {
    setTemplateType('');
    setIsActive('');
    setJournalId('');
  };

  return (
    <div>
      <PageHeader
        title="قوالب القيود"
        description="إدارة قوالب القيود اليومية"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/accounting/templates/create')}>
            إنشاء قالب
          </Button>
        }
      />
      <FilterBar hasFilters={hasFilters} onClear={clearAll}>
        <FilterSelect
          label="الدفتر"
          value={journalId}
          onChange={setJournalId}
          options={[
            { value: '', label: 'الكل' },
            ...journals.map((j) => ({ value: String(j.id), label: `${j.code} - ${j.name}` })),
          ]}
        />
        <FilterSelect
          label="النوع"
          value={templateType}
          onChange={setTemplateType}
          options={templateTypeOptions}
        />
        <FilterSelect
          label="الحالة"
          value={isActive}
          onChange={setIsActive}
          options={activeOptions}
        />
      </FilterBar>
      <div className="mt-4">
        <TemplateGrid
          data={templates}
          loading={isLoading}
          error={error ? 'فشل تحميل البيانات' : undefined}
          onRetry={() => refetch()}
        />
      </div>
    </div>
  );
}
