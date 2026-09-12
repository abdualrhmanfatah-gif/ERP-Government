import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, Badge, DataGrid, FilterBar, FilterSelect } from '@/components/ui';
import { useTemplatesList } from '../../hooks/useTemplatesList';
import { useJournalsList } from '../../hooks/useJournalsList';
import { JournalEntryTemplateType, type JournalEntryTemplateDto } from '../../../../web-api-client';
import { activeStatusLabels } from '@/shared/constants/labels';

const templateTypeLabels: Record<string, string> = {
  Standard: 'قياسية',
  Recurring: 'دورية',
  Adjustment: 'تسوية',
};

const columns = [
  { key: 'templateName', header: 'اسم القالب', accessorKey: 'templateName' as const, width: 200 },
  { key: 'journalName', header: 'الدفتر', accessorKey: 'journalName' as const, width: 150 },
  {
    key: 'templateType',
    header: 'النوع',
    accessorKey: 'templateType' as const,
    width: 120,
    cell: (row: JournalEntryTemplateDto) => templateTypeLabels[row.templateType ?? ''] ?? row.templateType ?? '—',
  },
  {
    key: 'isActive',
    header: 'نشط',
    accessorKey: 'isActive' as const,
    width: 80,
    cell: (row: JournalEntryTemplateDto) => (
      <Badge variant={row.isActive ? 'success' : 'default'}>
        {row.isActive ? 'نشط' : 'غير نشط'}
      </Badge>
    ),
  },
];

const templateTypeOptions = [
  { value: '', label: 'الكل' },
  { value: JournalEntryTemplateType.Standard, label: 'قياسية' },
  { value: JournalEntryTemplateType.Recurring, label: 'دورية' },
  { value: JournalEntryTemplateType.Adjustment, label: 'تسوية' },
];

const activeOptions = [
  { value: '', label: 'الكل' },
  { value: 'true', label: activeStatusLabels.active },
  { value: 'false', label: activeStatusLabels.inactive },
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
    <Page
      title="قوالب القيود"
      description="إدارة قوالب القيود اليومية"
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/accounting/templates/create')}>
          إنشاء قالب
        </Button>
      }
      toolbar={
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
      }
    >
      <DataGrid
        columns={columns}
        data={templates}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد قوالب"
        rowKey={(row) => row.id ?? 0}
        onRowClick={(row) => navigate(`/accounting/templates/${row.id}`)}
      />
    </Page>
  );
}
