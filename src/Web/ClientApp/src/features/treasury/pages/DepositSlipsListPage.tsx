import { useNavigate } from 'react-router-dom';
import { Button, Page, Card, CardContent, CardHeader, CardTitle } from '@/components/ui';
import { Banknote, Send } from 'lucide-react';

export default function DepositSlipsListPage() {
  const navigate = useNavigate();

  return (
    <Page title="بطاقات الإيداع">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card
          className="cursor-pointer hover:border-[var(--color-primary)] transition-colors"
          onClick={() => navigate('/treasury/deposit-slips/47/create')}
        >
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Banknote size={20} className="text-[var(--color-primary)]" />
              حافظة توريد النقد (47)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-[var(--color-on-surface-variant)]">
              توريد النقد للبنك — اختيار سندات القبض النقدية المعتمدة لتوليد قيد إخلاء عهدة الصندوق.
            </p>
            <Button variant="outline" size="sm" className="mt-4" onClick={(e) => { e.stopPropagation(); navigate('/treasury/deposit-slips/47/create'); }}>
              إنشاء حافظة 47
            </Button>
          </CardContent>
        </Card>

        <Card
          className="cursor-pointer hover:border-[var(--color-primary)] transition-colors"
          onClick={() => navigate('/treasury/deposit-slips/48/create')}
        >
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Send size={20} className="text-[var(--color-primary)]" />
              حافظة إرسال الشيكات (48)
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-[var(--color-on-surface-variant)]">
              إرسال الشيكات للتحصيل — اختيار الشيكات المستلمة لتحويلها إلى `UnderCollection` وتوليد قيد المقاصة.
            </p>
            <Button variant="outline" size="sm" className="mt-4" onClick={(e) => { e.stopPropagation(); navigate('/treasury/deposit-slips/48/create'); }}>
              إنشاء حافظة 48
            </Button>
          </CardContent>
        </Card>
      </div>
    </Page>
  );
}
