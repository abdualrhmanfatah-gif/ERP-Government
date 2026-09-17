# مهام التنفيذ: نظام إدارة وتوريد الإيرادات الحكومية

## نظرة عامة

**الميزة**: نظام إدارة وتوريد الإيرادات الحكومية (التحصيل الفعلي)
**التقييم**: الكيانات والأحداث والمعالجات الموجودة مطابقة بالكامل للمواصفة
**الapproach**: التحقق من الاكتمال وضمان الجودة (Verification & Validation)

---

## المرحلة 1: الإعداد (Setup)

- [x] T001 التحقق من بنية المشروع وتوافقها مع المواصفة — `src/Domain/Revenue/Entities/`
- [x] T002 التحقق من تسجيل الكيانات في DbContext — `src/Application/Common/Interfaces/IApplicationDbContext.cs`
- [x] T003 التحقق من وجود جميع Enums المطلوبة — `src/Domain/Revenue/Enums/`

---

## المرحلة 2: الأساسيات (Foundational)

- [x] T004 التحقق من ارث الكيانات لـ BaseAuditableEntity — `src/Domain/Revenue/Entities/RevenueClaim.cs`
- [x] T005 التحقق من وجود byte[] RowVersion في جميع الكيانات — `src/Domain/Revenue/Entities/`
- [x] T006 التحقق من صحة Relationships في DbContext — `src/Infrastructure/Data/ApplicationDbContext.cs`
- [x] T007 التأكد من أن RevenueJournalEntryService ي 지원 جميع الحسابات — `src/Application/Revenue/Common/Services/RevenueJournalEntryService.cs`

---

## المرحلة 3: سيناريو 1 — تحصيل نقدي [US1]

**الهدف**: التحقق من صحة سيناريو التحصيل النقدي بالكامل
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T008 [US1] التحقق من CreateRevenueClaimCommand — `src/Application/Revenue/Commands/RevenueClaims/CreateRevenueClaim/CreateRevenueClaimCommand.cs`
- [x] T009 [US1] التحقق من ApproveRevenueClaimCommand وسير الحالة — `src/Application/Revenue/Commands/RevenueClaims/ApproveRevenueClaim/ApproveRevenueClaimCommand.cs`
- [x] T010 [US1] التحقق من CreateCollectionOrderCommand والتحقق من المبلغ — `src/Application/Revenue/Commands/CollectionOrders/CreateCollectionOrder/CreateCollectionOrderCommand.cs`
- [x] T011 [US1] التحقق من ApproveCollectionOrderCommand — `src/Application/Revenue/Commands/CollectionOrders/ApproveCollectionOrder/ApproveCollectionOrderCommand.cs`
- [x] T012 [US1] التحقق من CreateReceiptVoucherCommand (نقدي) — `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs`
- [x] T013 [US1] التحقق من ApproveReceiptVoucherCommand (نقدي) — `src/Application/Revenue/Commands/ReceiptVouchers/ApproveReceiptVoucher/ApproveReceiptVoucherCommand.cs`
- [x] T014 [US1] التحقق من CashReceiptApprovedEventHandler — `src/Application/Revenue/EventHandlers/CashReceiptApprovedEventHandler.cs`
- [x] T015 [US1] التحقق من RevenueMetricsCalculator — `src/Application/Revenue/Common/Services/RevenueMetricsCalculator.cs`

---

## المرحلة 4: سيناريو 2 — تحصيل بالشيكات [US2]

**الهدف**: التحقق من صحة سيناريو تحصيل الشيكات بالكامل
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T016 [US2] التحقق من CreateReceiptVoucherCommand (شيك) — `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs`
- [x] T017 [US2] التحقق من CheckReceiptApprovedEventHandler — `src/Application/Revenue/EventHandlers/CheckReceiptApprovedEventHandler.cs`
- [x] T018 [US2] التحقق من CreateDepositSlip48Command — `src/Application/Revenue/Commands/DepositSlips/CreateDepositSlip48/CreateDepositSlip48Command.cs`
- [x] T019 [US2] التحقق من ApproveDepositSlip48Command — `src/Application/Revenue/Commands/DepositSlips/ApproveDepositSlip48/ApproveDepositSlip48Command.cs`
- [x] T020 [US2] التحقق من DepositSlip48ApprovedEventHandler — `src/Application/Revenue/EventHandlers/DepositSlip48ApprovedEventHandler.cs`
- [x] T021 [US2] التحقق من ClearCheckCommand — `src/Application/Revenue/Commands/Checks/ClearCheck/ClearCheckCommand.cs`
- [x] T022 [US2] التحقق من CheckClearedEventHandler (القيود المزدوجة) — `src/Application/Revenue/EventHandlers/CheckClearedEventHandler.cs`

---

## المرحلة 5: سيناريو 3 — ارتجاع الشيكات [US3]

**الهدف**: التحقق من صحة سيناريو ارتجاع الشيكات
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T023 [US3] التحقق من BounceCheckCommand — `src/Application/Revenue/Commands/Checks/BounceCheck/BounceCheckCommand.cs`
- [x] T024 [US3] التحقق من CheckBouncedEventHandler — `src/Application/Revenue/EventHandlers/CheckBouncedEventHandler.cs`
- [x] T025 [US3] التحقق من فك الحجز على المبلغ المتاح — `src/Application/Revenue/Common/Services/RevenueMetricsCalculator.cs`

---

## المرحلة 6: سيناريو 4 — توريد نقدي [US4]

**الهدف**: التحقق من صحة سيناريو توريد النقد للبنك
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T026 [US4] التحقق من CreateDepositSlip47Command — `src/Application/Revenue/Commands/DepositSlips/CreateDepositSlip47/CreateDepositSlip47Command.cs`
- [x] T027 [US4] التحقق من ApproveDepositSlip47Command — `src/Application/Revenue/Commands/DepositSlips/ApproveDepositSlip47/ApproveDepositSlip47Command.cs`
- [x] T028 [US4] التحقق من DepositSlip47ApprovedEventHandler — `src/Application/Revenue/EventHandlers/DepositSlip47ApprovedEventHandler.cs`

---

## المرحلة 7: سيناريو 5 — رقابة التحصيل الزائد [US5]

**الهدف**: التحقق من منع التحصيل الزائد
**riteria الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T029 [US5] التحقق من RevenueMetricsCalculator — AvailableAmount — `src/Application/Revenue/Common/Services/RevenueMetricsCalculator.cs`
- [x] T030 [US5] التحقق من validators تمنع التحصيل الزائد — `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs`
- [x] T031 [US5] التحقق من validators تمنع التحصيل الزائد (شيك) — `src/Application/Revenue/Commands/ReceiptVouchers/CreateReceiptVoucher/CreateReceiptVoucherCommand.cs`

---

## المرحلة 8: واجهات API [US6]

**الهدف**: التحقق من صحة جميع الـ endpoints
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T032 [US6] التحقق من RevenueClaims endpoints — `src/Web/Endpoints/Revenue/RevenueClaims.cs`
- [x] T033 [US6] التحقق من CollectionOrders endpoints — `src/Web/Endpoints/Revenue/CollectionOrders.cs`
- [x] T034 [US6] التحقق من ReceiptVouchers endpoints — `src/Web/Endpoints/Revenue/ReceiptVouchers.cs`
- [x] T035 [US6] التحقق من Checks endpoints — `src/Web/Endpoints/Revenue/Checks.cs`
- [x] T036 [US6] التحقق من DepositSlips endpoints — `src/Web/Endpoints/Revenue/DepositSlips.cs`

---

## المرحلة 9: الاختبارات [US7]

**الهدف**: تشغيل جميع الاختبارات والتحقق من اجتيازها
**معايير الاستقلالية**: يمكن اختبار هذا السيناريو بمفرده

- [x] T037 [US7] تشغيل Domain.UnitTests — `tests/Domain.UnitTests`
- [x] T038 [US7] تشغيل Application.UnitTests — `tests/Application.UnitTests`
- [x] T039 [US7] تشغيل Application.FunctionalTests — `tests/Application.FunctionalTests`
- [x] T040 [US7] تشغيل Infrastructure.IntegrationTests — `tests/Infrastructure.IntegrationTests`

---

## المرحلة 10: التحسين النهائي (Polish)

- [x] T041 التحقق من بنية الـ DTOs — `src/Application/Revenue/Common/DTOs/RevenueDtos.cs`
- [x] T042 التحقق من PermissionCodes — `src/Application/Common/Security/PermissionCodes.cs`
- [x] T043 التحقق من توثيق API — `src/Web/` (OpenAPI)
- [x] T044 بناء الواجهة الأمامية والتحقق — `src/Web/ClientApp`

---

## الرسم البياني للعلاقات (Dependency Graph)

```
T001-T007 (Setup + Foundational)
    │
    ├──→ T008-T015 (US1: تحصيل نقدي)
    │        │
    │        ├──→ T016-T022 (US2: تحصيل بالشيكات)
    │        │        │
    │        │        ├──→ T023-T025 (US3: ارتجاع شيك)
    │        │        │
    │        │        ├──→ T026-T028 (US4: توريد نقدي)
    │        │        │
    │        │        └──→ T029-T031 (US5: رقابة تحصيل زائد)
    │
    └──→ T032-T036 (US6: واجهات API)
             │
             └──→ T037-T040 (US7: اختبارات)
                      │
                      └──→ T041-T044 (Polish)
```

---

## الأمثلة على التنفيذ المتوازي

### الدفعة 1 (Setup + Foundational)
- T001, T002, T003 يمكن تنفيذها بالتوازي
- T004, T005, T006, T007 يمكن تنفيذها بالتوازي

### الدفعة 2 (User Stories)
- T008-T015 (US1) يجب أن تكتمل أولاً
- بعد US1: T016-T022 (US2) و T026-T028 (US4) يمكن تنفيذها بالتوازي
- بعد US2: T023-T025 (US3) و T029-T031 (US5) يمكن تنفيذها بالتوازي

### الدفعة 3 (API + Tests)
- T032-T036 (US6) يمكن تنفيذها بالتوازي
- T037-T040 (US7) يجب تنفيذها تسلسليًا

---

## نطاق MVP المقترح

**المبلغ الأدنى القابل للعيش (MVP)**: US1 فقط (تحويل نقدي)

**المحتوى**:
- T001-T007 (Setup + Foundational)
- T008-T015 (US1: تحصيل نقدي)
- T032-T034 (RevenueClaims + CollectionOrders + ReceiptVouchers endpoints)
- T037-T038 (Domain + Application UnitTests)

**النتيجة**: نظام يعمل بالكامل للتحويل النقدي مع قيود محاسبية تلقائية

---

## ملخص عدد المهام

| المرحلة | عدد المهام |
|---------|-----------|
| Setup | 3 |
| Foundational | 4 |
| US1: تحصيل نقدي | 8 |
| US2: تحصيل بالشيكات | 7 |
| US3: ارتجاع شيك | 3 |
| US4: توريد نقدي | 3 |
| US5: رقابة تحصيل زائد | 3 |
| US6: واجهات API | 5 |
| US7: اختبارات | 4 |
| Polish | 4 |
| Convergence | 5 |
| **المجموع** | **49** |

---

## المرحلة 11: التقارب (Convergence)

- [x] T045 إضافة أمر CancelReceiptVoucher و endpoint POST /{id}/cancel — الحالة `ReceiptVoucherStatus.Cancelled` وال字段 `CancellationReason` موجودتان لكن لا يوجد أمر أو endpoint لتفعيلهما (missing, FR-01)
- [x] T046 إضافة أمر WriteOffRevenueClaim و endpoint POST /{id}/writeoff — الحالة `ClaimStatus.WrittenOff` موجودة لكن لا يوجد أمر يُحوّل المطالبة لهذه الحالة (missing, FR-01)
- [x] T047 التحقق من تفويض FR-10التقارير إلى specs 020/044 أو إضافة استعلامات ملخصية — الاستعلام الحالية تُرجع قوائم فقط بدون تجميع إحصائيات (partial, FR-10)
- [x] T048 التحقق مما إذا كان Submit RevenueClaim/CollectionOrder مطلوبًا (Draft→PendingApproval) — أوامر الاعتماد تقبل Draft مباشرة (partial, spec lifecycle)
- [x] T049 مراجعة تطابق specs 031-receipt-vouchers مع التنفيذ الحالي — الحالة `PendingReview` وأمر `SubmitReceiptVoucher` غير موجودان في الكود (unrequested, TRE-01 spec 031)
