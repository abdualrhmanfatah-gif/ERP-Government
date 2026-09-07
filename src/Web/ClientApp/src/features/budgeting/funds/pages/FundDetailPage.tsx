import { useParams, useNavigate } from 'react-router-dom';
import { useFundDetail } from '../hooks/useFunds';
import { fundTypeLabels, fundCategoryLabels } from '../../shared/types';
import { Button, Badge, Card, Loading } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function FundDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const fundId = Number(id);

  const { data: fund, isLoading, error } = useFundDetail(fundId);

  if (isLoading) {
    return <Loading />;
  }

  if (error || !fund) {
    return <div className="p-6 text-center text-[var(--color-error)]">حدث خطأ أثناء تحميل بيانات الصندوق</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => navigate('/budgeting/funds')}
          aria-label="العودة"
        >
          <ArrowRight size={18} />
        </Button>
        <div>
          <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">تفاصيل الصندوق</h1>
          <p className="text-sm text-[var(--color-on-surface-variant)]">{fund.fundNumber} — {fund.fundName}</p>
        </div>
      </div>

      <Card className="bg-[var(--color-surface-container-lowest)]">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم الصندوق</span>
            <span className="block font-mono text-sm text-[var(--color-on-surface)]">{fund.fundNumber}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">اسم الصندوق</span>
            <span className="block text-sm text-[var(--color-on-surface)]">{fund.fundName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">النوع</span>
            <Badge variant="outline">{fundTypeLabels[fund.fundType]}</Badge>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الفئة</span>
            <Badge variant="outline">{fundCategoryLabels[fund.fundCategory]}</Badge>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الجهة القانونية</span>
            <span className="block text-sm text-[var(--color-on-surface)]">{fund.legalAuthority}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الحالة</span>
            <Badge variant={fund.isActive ? 'success' : 'danger'}>
              {fund.isActive ? 'نشط' : 'معطل'}
            </Badge>
          </div>
          {fund.fiscalYearId && (
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">السنة المالية</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{fund.fiscalYearId}</span>
            </div>
          )}
          {fund.description && (
            <div className="md:col-span-2">
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الوصف</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{fund.description}</span>
            </div>
          )}
        </div>
      </Card>
    </div>
  );
}
