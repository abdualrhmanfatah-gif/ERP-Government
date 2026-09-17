# دليل التحقق السريع: نظام إدارة وتوريد الإيرادات الحكومية

## المتطلبات الأساسية
- .NET 10 SDK
- SQL Server (محلي أو Docker)
- Node.js (للواجهة الأمامية)

## بناء المشروع

```bash
# بناء الخلفية
dotnet build src/Web/Web.csproj

# بناء الواجهة الأمامية
cd src/Web/ClientApp && npm run build
```

## سيناريوهات التحقق

### السيناريو 1: تحصيل نقدي كامل

**الم全流程**: إنشاء مطالبة → أمر تحصيل → سند قبض نقدي → توريد نقدي

**خطوات التحقق**:

1. **إنشاء مطالبة إيرادية**
   - POST `/api/RevenueClaims` مع البيانات المطلوبة
   - التحقق: يُرجع RevenueClaimDto برقم تسلسلي وstatus=Draft

2. **اعتماد المطالبة**
   - POST `/api/RevenueClaims/{id}/approve`
   - التحقق: status=Open

3. **إنشاء أمر تحصيل**
   - POST `/api/CollectionOrders` مع RevenueClaimId والمبلغ
   - التحقق: يُرجع CollectionOrderDto

4. **اعتماد أمر التحصيل**
   - POST `/api/CollectionOrders/{id}/approve`
   - التحقق: status=Approved

5. **إنشاء سند قبض نقدي**
   - POST `/api/ReceiptVouchers` مع CollectionOrderId وPaymentMethod=Cash
   - التحقق: يُرجع ReceiptVoucherDto

6. **اعتماد سند القبض**
   - POST `/api/ReceiptVouchers/{id}/approve`
   - التحقق:
     - status=Approved
     - **قيد محاسبي**: مدين 1812 (نقدية لدى أمين الصندوق) / دائن حساب الإيراد
     - حالة المطالبة: PartiallySettled أو Settled

**التحقق من منع التحصيل الزائد**:
- محاولة إنشاء سند قبض نقدي بمبلغ يتجاوز AvailableAmount
- التحقق: رسالة خطأ واضحة

---

### السيناريو 2: تحصيل بالشيكات + تصفية

**الم全流程**: سند قبض بالشيك → حافظة 48 → تصفية شيك

**خطوات التحقق**:

1. **إنشاء سند قبض بالشيك**
   - POST `/api/ReceiptVouchers` مع PaymentMethod=Check وبيانات الشيك
   - التحقق: status=Draft

2. **اعتماد سند القبض**
   - POST `/api/ReceiptVouchers/{id}/approve`
   - التحقق:
     - **قيد وسيط**: مدين 110201 (شيكات برسم الإيداع) / دائن 210901 (متحصلات معلقة)
     - حالة الشيك: Received
     - **لا يُعترف بالإيراد بعد**

3. **إنشاء حافظة 48**
   - POST `/api/DepositSlips/slip48` مع CheckIds
   - التحقق: يُرجع DepositSlip48Dto

4. **اعتماد حافظة 48**
   - POST `/api/DepositSlips/slip48/{id}/approve`
   - التحقق:
     - **قيد**: مدين 110202 (شيكات تحت التحصيل) / دائن 110201 (شيكات برسم الإيداع)
     - حالة الشيك: UnderCollection

5. **تسجيل تصفية الشيك**
   - POST `/api/Checks/{id}/clear`
   - التحقق:
     - **قيد 1**: مدين 110102 (بنك) / دائن 110202 (شيكات تحت التحصيل)
     - **قيد 2**: مدين 210901 (متحصلات معلقة) / دائن حساب الإيراد
     - حالة الشيك: Cleared
     - **هنا يُعترف بالإيراد الفعلي**

---

### السيناريو 3: ارتجاع شيك

**الم全流程**: سند قبض بالشيك → حافظة 48 → ارتجاع شيك

**خطوات التحقق**:

1. **仗例 التحقق من الخطوات 1-4 في السيناريو 2** (حتى UnderCollection)

2. **تسجيل ارتجاع الشيك**
   - POST `/api/Checks/{id}/bounce`
   - التحقق:
     - **قيد عكس**: مدين 210901 (متحصلات معلقة) / دائن 110202 (شيكات تحت التحصيل)
     - حالة الشيك: Bounced
     - **لا يُعكس إيراد** (لم يُعترف به)
     - يُفك الحجز على المبلغ

---

### السيناريو 4: رقابة التحصيل الزائد

**خطوات التحقق**:

1. إنشاء مطالبة بمبلغ 1000
2. إصدار سند قبض نقدي بمبلغ 600 → يُعتمد
3. محاولة إصدار سند قبض نقدي بمبلغ 500
   - المتاح = 1000 - 600 = 400
   - التحقق: **يُمنع** مع رسالة خطأ

---

## أوامر الاختبار السريع

```bash
# تشغيل اختبارات الخلفية
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Infrastructure.IntegrationTests

# اختبار الواجهة الأمامية
cd src/Web/ClientApp && npm run lint
```

## ملاحظات التحقق

- **القيود المحاسبية**: تتحقق تلقائيًا عند اعتماد السندات
- **المبالغ**: تُحسب ديناميكيًا من الحركات المحاسبية
- **السجل الرقابي**: كل حركة تُسجل في DocumentStatusLog
- **التعديل المتزامن**: RowVersion يمنع التعديل المتزامن
