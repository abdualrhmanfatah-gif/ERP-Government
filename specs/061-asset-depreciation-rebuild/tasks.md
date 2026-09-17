# Tasks: إعادة بناء ميزة الإهلاك

**Input**: Design documents from `/specs/061-asset-depreciation-rebuild/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: كيانات، ترحيل قاعدة البيانات، صلاحيات، أدوار القالب

- [ ] T001 [P] Create domain entities + enums: `DepreciationRun.cs`, `DepreciationScheduleLine.cs`, `DepreciationRunStatus.cs`, `DepreciationMethod.cs`, `MissedPeriodsPolicy.cs` in `src/Domain/Assets/Entities/` and `src/Domain/Assets/Enums/`
- [ ] T002 [P] Create EF configurations: `DepreciationRunConfiguration.cs`, `DepreciationScheduleLineConfiguration.cs` in `src/Infrastructure/Data/Configurations/Assets/` — فهارس فريدة (Year,Period) و(Asset,Period) ودقة 23,6
- [ ] T003 [P] Add `DepreciationScheduleLineId int?` FK + index to `JournalEntryLine` in `src/Domain/Accounting/Entities/JournalEntryLine.cs` and `src/Infrastructure/Data/Configurations/Accounting/JournalEntryLineConfiguration.cs`
- [ ] T004 [P] Change `Assets.AccumulatedDepreciation` and `Assets.CurrentValue` precision from `decimal(23,2)` to `decimal(23,6)` in `src/Infrastructure/Data/Configurations/Assets/AssetConfiguration.cs`
- [ ] T005 [P] Add `DepreciationExpense` and `AccumulatedDepreciation` values to `TemplateLineRole` enum in `src/Domain/Assets/Enums/TemplateLineRole.cs` (keep old values for backward compat or replace `GroupDepreciationAccount`)
- [ ] T006 Create migration: drop `AssetDepreciationRuns` + `DepreciationSchedules` + `AssetGroups.DepreciationAccountId`; create `DepreciationRuns` + `DepreciationScheduleLines`; alter `JournalEntryLine` + `Assets` columns; update template seed data
- [ ] T007 [P] Update permissions: remove `AssetDepreciationReverse` from `src/Application/Common/Security/PermissionCodes.cs`; update seeds in `src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs` and `src/Infrastructure/Data/Seeds/SecurityPermissionSeedData.cs`
- [ ] T008 [P] Update `DepreciationPostedHandler` in `src/Application/Accounting/Integration/Assets/DepreciationPostedHandler.cs` to use new roles (`DepreciationExpense`/`AccumulatedDepreciation`) + tag lines with `DepreciationScheduleLineId` instead of `AssetDepreciationRunId`
- [ ] T009 Update `IApplicationDbContext`: rename `AssetDepreciationRuns`→`DepreciationRuns`, `DepreciationSchedules`→`DepreciationScheduleLines`; remove old DbSet entries

**Checkpoint**: جداول جديدة + أعمدة محدثة + صلاحيات نظيفة + حدث الترحيل يدور بالجديد

---

## Phase 2: Foundational (Blocking)

**Purpose**: الحاسبة النقية، حل القالب، التحقق من الأهلية — BLOCKS كل US

- [ ] T010 Rewrite `DepreciationCalculator.cs` in `src/Domain/Assets/Services/DepreciationCalculator.cs` — حاسبة نقي بمدخلات لقطة (StraightLine) مع تقريب 6 منازل + إيقاف عند اكتمال/استبعاد
- [ ] T011 Update `DepreciationPostingTemplateResolver.cs` in `src/Application/Accounting/Common/DepreciationPostingTemplateResolver.cs` — الأدوار الجديدة: `DepreciationExpense` (مدين) و`AccumulatedDepreciation` (دائن)
- [ ] T012 Update `PostingGateValidator.cs` in `src/Application/Assets/Common/PostingGateValidator.cs` — التحقق من الأدوار الجديدة بدلا من حساب المجموعة
- [ ] T013 Create eligibility helper in `src/Application/Assets/Depreciation/Common/AssetEligibilityFilter.cs` — استعلام أصول مؤهلة: Active + IsDepreciable + غير مكتمل + عملة محلية + بلا استبعاد منفذ + تاريخ بدء ≤ تاريخ التشغيل + سياسة مكتملة

**Checkpoint**: حاسبة مختبرة + قالب يعمل بالأدوار الجديدة + أهلية قابلة للاستدعاء

---

## Phase 3: User Story 1 — التشغيل والمعاينة (Priority: P1) — MVP

**Goal**: محاسب يشغّل إهلاك فترة (استدراك أو حالية) ويحظّي ملخصا قبل الحفظ

**Independent Test**: تشغيل لفترة بـ 3 أصول مؤهلة + واحدة ناقصة → ملخص معاينة + مسودة محفوظة؛ تشغيل ثان لنفس الفترة → 400 DuplicateRun

### Implementation

- [ ] T014 [US1] Create `RunDepreciationCommand` record + `RunDepreciationCommandValidator` in `src/Application/Assets/Depreciation/Run/Commands/RunDepreciationCommand.cs` — الحقول: FiscalYearId, FiscalPeriodId, DepreciationDate, MissedPeriodsPolicy, Notes
- [ ] T015 [US1] Create `PreviewDepreciationQuery` in `src/Application/Assets/Depreciation/Preview/Queries/PreviewDepreciationQuery.cs` — نفس مدخلات Run لكن لا يحفظ (قراءة فقط)
- [ ] T016 [US1] Implement `RunDepreciationCommandHandler` in `src/Application/Assets/Depreciation/Run/Commands/RunDepreciationCommand.cs` — فحص الفترة، الأهلية، الاستدراق، التكرار، إنشاء الرأس + السجلات، الحفظ الدفعي
- [ ] T017 [US1] Implement `PreviewDepreciationQueryHandler` — يقرأ الأصول المؤهلة، يحسب بالحاسبة النقية، يعيد ملخصا (إجمالي/عدد/أعلى 10/استثناءات) بلا حفظ
- [ ] T018 [US1] Create `DeleteRunCommand` in `src/Application/Assets/Depreciation/Run/Commands/DeleteRunCommand.cs` — حذف مسودة فقط (Draft) مع Cascade
- [ ] T019 [US1] Rewrite endpoints in `src/Web/Endpoints/Assets/AssetDepreciation.cs` — POST preview, POST runs, GET runs, GET runs/{id}, DELETE runs/{id} مع الصلاحيات المحدثة
- [ ] T020 [US1] Update frontend types + hooks in `src/Web/ClientApp/src/features/assets/asset-depreciation/` — `types.ts` (RunDepreciationRequest مع MissedPeriodsPolicy، RunDetailResponse مع scheduleLines)، `useDepreciation.ts` (preview hook، delete hook)
- [ ] T021 [US1] Rewrite `DepreciationListPage.tsx` — تشغيل بحوار يجمع السنة/الفترة/التاريخ/السياسة/ملاحظات؛ عرض المعاينة داخل الحوار قبل الحفظ
- [ ] T022 [US1] Rewrite `DepreciationDetailPage.tsx` — عرض السجلات (جدول + ملخص) + زر ترحيل (Draft) + زر حذف (Draft) + رابط القيد (Posted) — بلا زر عكس

**Checkpoint**: تشغيل + معاينة + مسودة + عرض + حذف مسودة — كلها تعمل من الواجهة

---

## Phase 4: User Story 3 — الترحيل (Priority: P1)

**Goal**: محاسب يرحّل مسودة → قيد محاسبي موسوم بسجل الأصل + تحديث البطاقة

**Independent Test**: ترحيل مسودة → قيد متوازن، كل سطر بـ DepreciationScheduleLineId، البطاقة محدثة، الحالة Posted

### Implementation

- [ ] T023 [US3] Create `PostDepreciationCommand` + handler in `src/Application/Assets/Depreciation/Post/Commands/PostDepreciationCommand.cs` — فحص Draft + الفترة + القالب + الحسابات، ضبط Posting + حدث DepreciationPosted
- [ ] T024 [US3] Rewrite `DepreciationPostedHandler` consumer logic: بناء القيد بزوج سطور لكل سجل (مدين مصروف / دائن مجمع)، وسم كل سطر بـ `DepreciationScheduleLineId`، تحديث البطاقات (المتراكم + آخر تاريخ + اكتمال بدقة 23,6)، تحديث الرأس Posted + JournalEntryId — idempotent
- [ ] T025 [US3] Wire post action in frontend: زر الترحيل في `DepreciationDetailPage.tsx` + حالة `Posting` تظهر "قيد الترحيل" مع انتظار التحديث

**Checkpoint**: الترحيل ينتج قيدا موسوما، البطاقة محدثة، لا تكرار

---

## Phase 5: Polish & Cross-Cutting (P2 — US4)

**Goal**: تتبع كامل + اختبار أداء + تنظيف

### Implementation

- [ ] T026 [US4] Add reconciliation check endpoint or test in `tests/Application.FunctionalTests/Assets/Depreciation/` — فحص رصيد البطاقة = مجموع اللقطات المرحلة لكل الأصول في العملية
- [ ] T027 [P] Fix acceptance page path in `tests/Web.AcceptanceTests/Pages/DepreciationPage.cs:5` — من `/asset-depreciation` إلى `/assets/depreciation`
- [ ] T028 [P] Add performance acceptance test in `tests/Application.FunctionalTests/Assets/Depreciation/DepreciationPerformanceTests.cs` — seed 10k أصول، قياس تشغيل + ترحيل < 30 ث
- [ ] T029 [P] Remove all obsolete reverse-related code: `ReverseDepreciationCommand.cs`, `ReverseDepreciationRequest` DTO, `AssetDepreciationReverse` permission references, reverse button in UI, `DepreciationReversed.cs` event if unused
- [ ] T030 Run full quickstart.md validation: build + domain tests + unit tests + functional tests + acceptance tests

**Checkpoint**: كل معايير القبول في spec.md تتحقق

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — starts immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion (T001-T009)
- **Phase 3 (US1 — Run)**: Depends on Phase 2 completion (T010-T013)
- **Phase 4 (US3 — Post)**: Depends on Phase 1 (T008 handler update) + Phase 2 (T011-T012); can parallel Phase 3
- **Phase 5 (Polish)**: Depends on Phase 3 + Phase 4 completion

### Parallel Opportunities

| Phase | Parallel Group |
|-------|---------------|
| Phase 1 | T001 + T002 + T003 + T004 + T005 + T007 + T009 (all [P]) |
| Phase 2 | T010 + T011 + T012 (all [P] — different files) |
| Phase 3 | T020 + T021 + T022 (frontend tasks can parallel backend if separate dev) |
| Phase 5 | T027 + T028 + T029 (all [P]) |

---

## Implementation Strategy

### MVP First (US1 — Run + Preview)

1. Phase 1: جداول + أعمدة + صلاحيات
2. Phase 2: حاسبة + قالب + أهلية
3. Phase 3: تشغيل + معاينة + عرض
4. **STOP & VALIDATE**: معاينة + تشغيل + مسودة من الواجهة

### Incremental Delivery

1. Setup + Foundational → جاهز
2. US1 (Run + Preview + List) → MVP يعمل
3. US3 (Post + Event Consumer) → ترحيل كامل
4. Polish → اختبارات + أداء + تنظيف

---

## Notes

- المهمات مبنية على الوثائق الخمسة المولّدة (plan/research/data-model/contracts/quickstart)
- لا ت exist في spec.md طلب صريح لـ TDD → اختبارات الوحدة/التكامل متروكة للتنفيذ عند الحاجة
- اختبار الأداء (T028) إلزامي وفق معيار القبول 6 في spec.md
- الجداول القديمة تُسقط في T006 (migration) — لا ترحيل بيانات
