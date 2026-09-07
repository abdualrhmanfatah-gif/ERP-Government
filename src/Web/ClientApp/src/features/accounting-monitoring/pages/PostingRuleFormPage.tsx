import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePostingRule, useCreatePostingRule, useUpdatePostingRule } from '../hooks/usePostingRules';
import { notify } from '@/features/notifications/notify';
import type { PostingRuleLineDto } from '../shared/types';
import { Button, Card, Input, Select, Loading } from '@/components/ui';

interface LineForm extends Partial<PostingRuleLineDto> {
  _key: number;
}

const emptyLine = (): LineForm => ({
  _key: Date.now(),
  sequence: 0,
  accountSource: 'Fixed',
  fixedAccountId: null,
  fixedAccountCode: null,
  debitOrCredit: 'Debit',
  amountSource: 'DocumentAmount',
  fundDimensionRequired: false,
  costCenterDimensionRequired: false,
  projectDimensionRequired: false,
  isActive: true,
});

export function PostingRuleFormPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);

  const { data: existing, isLoading: loadingExisting } = usePostingRule(Number(id));
  const createMut = useCreatePostingRule();
  const updateMut = useUpdatePostingRule();

  const [name, setName] = useState('');
  const [eventType, setEventType] = useState('');
  const [journalId, setJournalId] = useState(0);
  const [priority, setPriority] = useState(0);
  const [isActive, setIsActive] = useState(true);
  const [lines, setLines] = useState<LineForm[]>([emptyLine()]);

  useEffect(() => {
    if (existing && isEdit) {
      setName(existing.name);
      setEventType(existing.eventType);
      setJournalId(existing.journalId);
      setPriority(existing.priority);
      setIsActive(existing.isActive);
      setLines(
        (existing.lines ?? []).map((l, i) => ({
          ...l,
          _key: i,
        })),
      );
    }
  }, [existing, isEdit]);

  const addLine = () => setLines((prev) => [...prev, emptyLine()]);
  const removeLine = (key: number) => setLines((prev) => prev.filter((l) => l._key !== key));

  const updateLine = (key: number, field: keyof LineForm, value: unknown) => {
    setLines((prev) => prev.map((l) => (l._key === key ? { ...l, [field]: value } : l)));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      name,
      eventType,
      journalId,
      priority,
      isActive,
      lines: lines.map((l, i) => ({
        ...l,
        sequence: i + 1,
        postingRuleId: Number(id) || 0,
        id: l.id || 0,
      })),
    };

    try {
      if (isEdit) {
        await updateMut.mutateAsync({ id: Number(id), command: payload as never });
        notify({ type: 'success', title: 'تم تحديث القاعدة بنجاح' });
      } else {
        await createMut.mutateAsync(payload as never);
        notify({ type: 'success', title: 'تم إنشاء القاعدة بنجاح' });
      }
      navigate('/accounting/posting-rules');
    } catch (e: unknown) {
      notify({ type: 'error', title: e instanceof Error ? e.message : 'فشل الحفظ' });
    }
  };

  if (isEdit && loadingExisting) {
    return (
        <div className="max-w-4xl mx-auto py-8 px-6" dir="rtl">
          <Loading />
        </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto py-8 px-6" dir="rtl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-[var(--color-on-surface)]">
          {isEdit ? 'تعديل قاعدة الترحيل' : 'قاعدة ترحيل جديدة'}
        </h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <Card variant="default" padding="md">
          <h2 className="text-sm font-bold text-[var(--color-on-surface)]">البيانات الأساسية</h2>
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="اسم القاعدة"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <Input
              label="نوع الحدث"
              type="text"
              value={eventType}
              onChange={(e) => setEventType(e.target.value)}
              required
            />
            <Input
              label="رقم اليومية"
              type="number"
              value={journalId}
              onChange={(e) => setJournalId(Number(e.target.value))}
              required
            />
            <Input
              label="الأولوية"
              type="number"
              value={priority}
              onChange={(e) => setPriority(Number(e.target.value))}
              required
            />
          </div>
          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="rounded"
            />
            <label className="text-sm font-bold text-[var(--color-on-surface)]">نشط</label>
          </div>
        </Card>

        <Card variant="default" padding="md">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-bold text-[var(--color-on-surface)]">بنود القاعدة</h2>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={addLine}
            >
              + بند
            </Button>
          </div>

          {lines.map((line, idx) => (
            <Card key={line._key} variant="flat" padding="sm" className="space-y-3 bg-[var(--color-surface-container-low)] border-[var(--color-outline-variant)]">
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold text-[var(--color-on-surface-variant)]">بند {idx + 1}</span>
                {lines.length > 1 && (
                  <Button
                    type="button"
                    variant="destructive"
                    size="xs"
                    onClick={() => removeLine(line._key)}
                  >
                    حذف
                  </Button>
                )}
              </div>
              <div className="grid grid-cols-3 gap-3">
                <Select
                  label="مصدر الحساب"
                  value={line.accountSource ?? 'Fixed'}
                  onChange={(e) => updateLine(line._key, 'accountSource', e.target.value)}
                  options={[
                    { value: 'Fixed', label: 'ثابت' },
                    { value: 'SourceDocumentAccount', label: 'حساب الوثيقة' },
                  ]}
                />
                <Select
                  label="مدين / دائن"
                  value={line.debitOrCredit ?? 'Debit'}
                  onChange={(e) => updateLine(line._key, 'debitOrCredit', e.target.value)}
                  options={[
                    { value: 'Debit', label: 'مدين' },
                    { value: 'Credit', label: 'دائن' },
                  ]}
                />
                <Select
                  label="المصدر"
                  value={line.amountSource ?? 'DocumentAmount'}
                  onChange={(e) => updateLine(line._key, 'amountSource', e.target.value)}
                  options={[
                    { value: 'DocumentAmount', label: 'مبلغ الوثيقة' },
                    { value: 'FixedAmount', label: 'مبلغ ثابت' },
                  ]}
                />
              </div>
              <div className="flex gap-4">
                <label className="flex items-center gap-1 text-xs font-bold text-[var(--color-on-surface)]">
                  <input type="checkbox" checked={line.fundDimensionRequired ?? false} onChange={(e) => updateLine(line._key, 'fundDimensionRequired', e.target.checked)} />
                  الصندوق
                </label>
                <label className="flex items-center gap-1 text-xs font-bold text-[var(--color-on-surface)]">
                  <input type="checkbox" checked={line.costCenterDimensionRequired ?? false} onChange={(e) => updateLine(line._key, 'costCenterDimensionRequired', e.target.checked)} />
                  مركز التكلفة
                </label>
                <label className="flex items-center gap-1 text-xs font-bold text-[var(--color-on-surface)]">
                  <input type="checkbox" checked={line.projectDimensionRequired ?? false} onChange={(e) => updateLine(line._key, 'projectDimensionRequired', e.target.checked)} />
                  المشروع
                </label>
              </div>
            </Card>
          ))}
        </Card>

        <div className="flex gap-3">
          <Button
            type="submit"
            variant="primary"
            disabled={createMut.isPending || updateMut.isPending}
            loading={createMut.isPending || updateMut.isPending}
          >
            {isEdit ? 'حفظ التعديلات' : 'إنشاء القاعدة'}
          </Button>
          <Button
            type="button"
            variant="ghost"
            onClick={() => navigate('/accounting/posting-rules')}
          >
            إلغاء
          </Button>
        </div>
      </form>
    </div>
  );
}
