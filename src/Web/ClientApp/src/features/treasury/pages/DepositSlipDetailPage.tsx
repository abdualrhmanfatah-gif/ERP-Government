// Deposit slip detail — placeholder until GET endpoint is added to API
import { useNavigate } from 'react-router-dom';
import { Button, Page, EmptyState } from '@/components/ui';

export default function DepositSlipDetailPage() {
  const navigate = useNavigate();

  return (
    <Page
      title="تفاصيل بطاقة الإيداع"
      onBack={() => navigate('/treasury/deposit-slips')}
    >
      <EmptyState message="لا توجد تفاصيل متاحة — API لا يوفر نقطة نهاية لجلب بيانات البطاقة. يرجى استخدام صفحة الإنشاء." />
      <div className="flex justify-center mt-4">
        <Button variant="outline" onClick={() => navigate('/treasury/deposit-slips')}>
          العودة لقائمة البطاقات
        </Button>
      </div>
    </Page>
  );
}
