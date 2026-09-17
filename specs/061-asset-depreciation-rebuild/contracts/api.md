# API Contract: `/api/AssetDepreciation`

**Date**: 2026-09-17 | الأخطاء: ProblemDetails موحد (051) + ErrorCodes عربية | الصلاحيات عبر `RequireAuthorization`

## Endpoints

| Method | Route | صلاحية | Response |
|--------|-------|--------|----------|
| POST | `/api/AssetDepreciation/preview` | `AssetDepreciation.View` | `Result<DepreciationPreviewResponse>` (قراءة فقط، لا حفظ) |
| POST | `/api/AssetDepreciation/runs` | `AssetDepreciation.Run` | `Result<RunDepreciationResult>` (مسودة) |
| GET | `/api/AssetDepreciation/runs` | `AssetDepreciation.View` | `PaginatedList<DepreciationRunResponse>` (فلاتر: fiscalYearId, status) |
| GET | `/api/AssetDepreciation/runs/{id}` | `AssetDepreciation.View` | `Result<DepreciationRunDetailResponse>` |
| DELETE | `/api/AssetDepreciation/runs/{id}` | `AssetDepreciation.Run` | `Result<int>` (مسودة فقط) |
| POST | `/api/AssetDepreciation/runs/{id}/post` | `AssetDepreciation.Post` | `Result<int>` |

ملاحظة: endpoint العكس محذوف مع الصلاحية (لا ميزة عكس).

## Payloads

```ts
// POST /preview  = POST /runs (نفس الطلب، بلا حفظ)
interface DepreciationComputeRequest {
  fiscalYearId: number;
  fiscalPeriodId: number;
  depreciationDate: string;          // yyyy-MM-dd
  missedPeriodsPolicy: 'CatchUp' | 'CurrentPeriodOnly';
  notes?: string;
}

interface RunDepreciationResult {
  runId: number;
  runNumber: string;
  assetsCount: number;
  totalDepreciation: number;
  schedulesCount: number;             // قد يزيد عن عدد الأصول (استدراق)
}

interface DepreciationPreviewResponse {
  assetsCount: number;
  schedulesCount: number;
  totalDepreciation: number;
  zeroAmountAssetsCount: number;
  topAssets: [{ assetId: number; code: string; name: string; amount: number }];  // أعلى 10
  excludedAssets: [{ assetId: number; code: string; reason: string }];           // أجنبية/مستبعدة/بلا بدء
}

interface DepreciationRunResponse {          // قائمة
  id: number; runNumber: string; fiscalYear: string; periodNumber: number;
  depreciationDate: string; missedPeriodsPolicy: 'CatchUp' | 'CurrentPeriodOnly';
  assetsCount: number; totalDepreciation: number;
  status: 'Draft' | 'Posting' | 'Posted'; journalEntryId?: number;
}

interface DepreciationScheduleLineResponse {
  id: number; assetId: number; assetCode: string; assetName: string;
  fiscalPeriodId: number; periodNumber: number; depreciationDate: string;
  method: string; rate: number; periodNumber: number; totalPeriods?: number;
  depreciationBase: number; residualValue: number;
  openingAccumulatedDepreciation: number; amount: number;
  closingAccumulatedDepreciation: number; closingBookValue: number;
  journalEntryLineId?: number;   // بعد الترحيل — رابط التتبع المباشر
}

interface DepreciationRunDetailResponse extends DepreciationRunResponse {
  notes?: string; journalEntryId?: number;
  postedAt?: string; postedBy?: string;
  rowVersion: string;
  scheduleLines: DepreciationScheduleLineResponse[];
}
```

## Error Codes (Arabic messages, 051)

| Code | HTTP | الحالة |
|------|------|--------|
| `Depreciation.FiscalPeriodNotFound` | 400/404 | الفترة غير موجودة أو لا تنتمي للسنة |
| `Depreciation.PeriodClosed` | 400 | الفترة مغلقة أو غير مفعلة |
| `Depreciation.DepreciationDateOutsidePeriod` | 400 | التاريخ خارج الفترة |
| `Depreciation.DuplicateRun` | 400 | توجد عملية إهلاك لهذه الفترة |
| `Depreciation.NoEligibleAssets` | 400 | لا أصول مؤهلة |
| `Depreciation.InvalidAssetPolicy` | 400 | إعدادات ناقصة + أسماء الأصول |
| `Depreciation.RunNotFound` | 404 | العملية غير موجودة |
| `Depreciation.InvalidStatusTransition` | 400 | ترحيل غير مسودة / حذف غير مسودة |
| `Depreciation.NoSchedulesToPost` | 400 | لا سجلات/إجمالي صفر |
| `Depreciation.TemplateNotConfigured` | 400 | القالب أو الدوران غير مهيأ |
| `Accounting.AccountNotPostable` | 400 | حساب القالب غير قابل للترحيل |
| `Accounting.ConcurrencyConflict` | 409 | تعارض RowVersion |

## Frontend Contract

- مسارات: `/assets/depreciation` (قائمة) و`/assets/depreciation/:id` (تفاصيل) — صفحة القبول تصلح لنفس المسارين.
- client يولد عبر nswag (`web-api-client.ts`) — لا استدعاء يدوي.
- الحالات: Draft (زر حذف+ترحيل)، Posting (انتظار)، Posted (عرض فقط + رابط القيد) — بلا زر عكس.
