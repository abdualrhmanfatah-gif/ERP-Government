# Quickstart: إعادة بناء ميزة الإهلاك

**Date**: 2026-09-17 | تفاصيل العقد: [contracts/api.md](contracts/api.md) | النموذج: [data-model.md](data-model.md)

## متطلبات سابقة

1. قاعدة بيانات SQL Server مطبق عليها آخر migration.
2. بيانات أساسية: سنة مالية بـ 3+ فترات، عملة محلية، حساب مصروف + حساب مجمع إهلاك قابلان للترحيل، مستخدم بصلاحيات `AssetDepreciation.View/Run/Post`.
3. قالب قيد مهيأ: `SystemTemplateKeys.AssetDepreciation` بسطرين — `DepreciationExpense` (مدين) و`AccumulatedDepreciation` (دائن).

## إعداد بيانات الاختبار (أدنى مجموعة)

1. مجموعة أصول: IsDepreciable=true، Method=قسط ثابت، Rate=10%، ResidualValuePercentage=0.
2. أصل A: مُفعّل، OriginalValue=120,000، بلا بدء خاص (يبدأ من أول فترة).
3. أصل B: OriginalValue=50,000، بسياسة ناقصة (Rate فارغ) — لتجربة الرفض.
4. أصل C: بعملة أجنبية — يظهر في قائمة الاستثناءات.

## سيناريو تحقق النهاية للنهاية

| # | الخطوة | المتوقع |
|---|--------|---------|
| 1 | POST `/api/AssetDepreciation/preview` لفترة الحالية (CatchUp، التاريخ داخل الفترة) | ملخص: 2 أصل مؤهل (B يستثنى)، الإجمالي يتسق مع المعادلات — لا حفظ |
| 2 | نفس الطلب على `/runs` | `runId` + `runNumber` بصيغة DEP؛ الحالة Draft |
| 3 | POST `/runs` مرة ثانية لنفس الفترة | 400 `DuplicateRun` برسالة عربية |
| 4 | تشغيل فترة فيها فترات مفتوحة فائتة + `CatchUp` | سجل لكل فترة فائتة + الحالية؛ `periodNumber` قياسي؛ إجمال واحد |
| 5 | نفس الفترة مع `CurrentPeriodOnly` (تشغيل جديدة بعد حذف الأولى DELETE `/runs/{id}` — مسودة) | سجل للفترة الحالية فقط |
| 6 | POST `/runs/{id}/post` | 200؛ الحالة تمر Posting → Posted بعد معالجة الحدث (Outbox) |
| 7 | فحص القيد الناتج | متوازن؛ زوج سطور لكل أصل؛ كل سطر يحمل `DepreciationScheduleLineId`؛ الحسابات من القالب منسوخة |
| 8 | فحص بطاقة الأصل | المتراكم/آخر تاريخ/الاكتمال محدثة بدقة 23,6 |
| 9 | إعادة تسليم حدث الترحيل (outbox re-drive) | لا قيد ثان ولا مضاعفة رصيد |
| 10 | ترحيل مسودة في فترة مغلقة / ثاني مرة | 400 برسالة عربية |
| 11 | أصول بسياسة ناقصة (B) ضمن تشغيل لا يعاملها استثناء | رفض `InvalidAssetPolicy` + اسم الأصل |
| 12 | فحص التوافق | رصيد البطاقة = مجموع اللقطات المرحلة — يمر |

## أوامر التحقق

```powershell
# بناء واختبارات
dotnet build
dotnet test tests/Domain.UnitTests --filter "FullyQualifiedName~Depreciation"
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Depreciation"
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~Assets"

# الأداء (10k أصول < 30 ث)
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~DepreciationPerformance"

# القبول (واجهة)
dotnet test tests/Web.AcceptanceTests --filter "FullyQualifiedName~Depreciation"
```

## علامات الفشل الشائعة

- قيد غير متوازن أو سطور بلا وسم → تحقق من أدوار القالب (data-model §3).
- الوسم موجود والرصيد مضاعف → idempotency مكسور (R12).
- زمن التشغيل > 30 ث لـ 10k → استعلام لكل أصل تسلل (R8/R10).
