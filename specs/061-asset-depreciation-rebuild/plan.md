# Implementation Plan: إعادة بناء ميزة الإهلاك

**Branch**: `061-asset-depreciation-rebuild` | **Date**: 2026-09-17 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/061-asset-depreciation-rebuild/spec.md`

## Summary

محرك إهلاك أصول جديد يستبدل نموذج Run-level الحالي كليا: سياسة إهلاك لكل مجموعة (طريقة/نسبة/تخريدي/قابل للإهلاك) مع تجاوز العمر وتاريخ البدء على الأصل، تشغيل دوري (مسودة → معاينة → ترحيل) يدعم الاستدراق أو الفترة الحالية فقط بقرار المستخدم، ترحيل قيد عبر قالب نظامي (مدين مصروف الإهلاك / دائن مجمع الإهلاك) بتسليم عبر الحدث الصادرة (Outbox)، وسم كل سطر قيد بسجل الإهلاك، وذرّية كاملة للترحيل، بلا عكس، بلا ترحيل بيانات قديمة (الجداول القديمة تُسقط). الأساس من الكلفة فقط، والعملة المحلية فقط في v1.

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`, SDK 10.0.201) — خادم. TypeScript 5 / React 18 + Vite — واجهة.

**Primary Dependencies**: EF Core (SQL Server) + MediatR (vertical slices) + FluentValidation +(outbox events pattern الحالي) + TanStack Query + nswag generated client. إعادة استخدام: `IApplicationDbContext`, `IDocumentSequenceService`, `Result<T>/ErrorCodes`, `PostingGateValidator`, `JournalEntryTemplates.LineRole`, صلاحيات `PermissionCodes`.

**Storage**: SQL Server (Entity Framework Core migrations).

**Testing**: xUnit — `tests/Domain.UnitTests` (الحاسبة النقي)، `tests/Application.UnitTests` (handlers)، `tests/Application.FunctionalTests` (مسارات API بالكامل)، `tests/Web.AcceptanceTests` (Playwright، صفحة الإهلاك).

**Target Platform**: ASP.NET Core minimal API (net10) + React SPA.

**Project Type**: web-service + SPA (حزمة واحدة `src/` بخدمات موجودة).

**Performance Goals**: تشغيل + ترحيل 10,000+ أصول < 30 ثانية (معيار قبول 6 في spec).

**Constraints**: كل رسالة خطأ عربية وفق `specs/051-unified-error-handling`؛ لا قيد غير متوازن؛ لا حالة نصفية؛ سجلات مرحلة غير قابلة للتعديل؛ idempotent لإعادة تسليم الحدث.

**Scale/Scope**: عملية إهلاك لكل (سنة، فترة)؛ سجل لكل أصل لكل فترة؛ إحلال جدولي `AssetDepreciationRuns`/`DepreciationSchedules` الحاليين بكيفين جددين.

## Constitution Check

لا يوجد `.specify/memory/constitution.md`. الحاكمية الفعلية للريبو (من AGENTS.md وspecs 051/052/057):

| Gate | الحالة | ملاحظة |
|------|--------|--------|
| عقد الأخطاء الموحد (051) | PASS (تصميم) | كل رفض `Result.Failure` بـ ErrorCodes + رسالة عربية؛ اختبار عقود المشكلات موجود |
| لغة الواجهة الموحدة (052) + أنماط أصول 057 | PASS (تصميم) | إعادة استخدام FilterBar/StatusBadge/Page recipes الحالية |
| تتبع الأثر سطر القيد ← سجل الإهلاك | PASS (تصميم) | عمود مرفق جديد `DepreciationScheduleLineId` |
| Idempotency للحدث | PASS (تصميم) | البحث عن قيد موجود قبل الإنشاء + حالة `Posting` |
| حماية التعارض | PASS (تصميم) | RowVersion على الرأس والسجلات |
| حذف بيانات قديمة | PASS (تصميم) | قرار المستخدم: تُهجر — ترحيل migration إسقاط/بناء |

## Project Structure

### Documentation (this feature)

```text
specs/061-asset-depreciation-rebuild/
├── plan.md              # هذا الملف
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/           # Phase 1
│   └── api.md
└── tasks.md             # /speckit.tasks لاحقا
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Assets/
│   │   ├── Entities/DepreciationRun.cs            # جديد (يستبدل AssetDepreciationRun)
│   │   ├── Entities/DepreciationScheduleLine.cs   # جديد (يستبدل DepreciationSchedule)
│   │   ├── Enums/DepreciationRunStatus.cs         # Draft/Posting/Posted (بلا Reversing/Reversed)
│   │   ├── Enums/DepreciationMethod.cs            # StraightLine (قائمة قابلة للامتداد)
│   │   ├── Enums/MissedPeriodsPolicy.cs           # CatchUp/CurrentPeriodOnly
│   │   └── Services/DepreciationCalculator.cs     # إعادة كتابة: نقي، بمدخلات لقطة
│   └── Events/Assets/DepreciationPosted.cs        # تعديل الحمولة
├── Application/
│   ├── Assets/Depreciation/
│   │   ├── Run/Commands/RunDepreciationCommand.cs       # إعادة كتابة (+MissedPeriodsPolicy)
│   │   ├── Preview/Queries/PreviewDepreciationQuery.cs  # جديد (قراءة فقط)
│   │   ├── Post/Commands/PostDepreciationCommand.cs     # إعادة كتابة
│   │   └── Queries/ (GetRuns, GetRunById)               # إعادة توجيه للكيانات الجديدة
│   ├── Assets/Common/PostingGateValidator.cs            # إعادة استخدام (تعديل roles القالب)
│   └── Accounting/Common/DepreciationPostingTemplateResolver.cs # إعادة استخدام
├── Infrastructure/
│   ├── Data/Configurations/Assets/ (الجديدان)     # دقة 23,6، فهارس فريدة
│   └── Data/Migrations/                            # إسقاط الجدولين القديمين + بناء الجديدين
├── Web/
│   └── Endpoints/Assets/AssetDepreciation.cs       # routes: runs (GET/POST), runs/{id} (GET), runs/{id}/post (POST), preview (POST)
└── Web/ClientApp/src/features/assets/asset-depreciation/  # إعادة كتابة الواجهة (contract جديد)

tests/
├── Domain.UnitTests/Assets/Depreciation/           # الحاسبة: تقريب، إيقاف، استدراق
├── Application.UnitTests/Assets/Depreciation/      # eligibility، تكرار، gates
├── Application.FunctionalTests/Assets/Depreciation/ # مسارات كاملة + idempotency + performance smoke
└── Web.AcceptanceTests/Pages/DepreciationPage.cs   # إصلاح مسار الصفحة إلى /assets/depreciation
```

**Structure Decision**: نفس التشريح العمودي الموجود (Domain/Application/Infrastructure/Web) — الإهلاك شريحة مستقلة داخل `Assets`، مع إعادة استخدام النظام المالي (فترات/قيود/قوالب/تسلسل/أخطاء). لا مشاريع جديدة.

## Complexity Tracking

لا انتهاكات حاكمية تحتاج تبريرا.

