# Government Disbursements

Language for documents that authorize and execute government expenditure.

## Revenue Collection

**Cashier Cash Account (نقدية لدى أمين الصندوق)**:
Asset account `1812` used for cash physically held by the cashier after a cash receipt is approved and before Form 47 deposits it to the bank. It is debited on cash revenue collection and credited when the cashier's custody is cleared by deposit.
_Avoid_: Revenue account, bank account, `110101`

**Beneficiary (المستفيد)**:
Person or party for whose benefit a disbursement is authorized. Their name remains part of the historical authorization; they may be registered as a Party or identified by name alone. The beneficiary may differ from the person submitting the request.
_Avoid_: Requester, supplier (unless describing a procurement supplier specifically)

**Accrual Journal Entry (قيد الاستحقاق)**:
Journal entry created when a disbursement request is approved, recognizing the expense and liability. Linked to the disbursement request via `AccrualJournalEntryId`. Created manually by the user from the disbursement request detail page.
_Avoid_: Accrual entry, accrual

## Language

**Disbursement Request (طلب الصرف)**:
Request to authorize a specific beneficiary and amount for expenditure. It precedes a payment order for request-led expenditure.
_Avoid_: Payment, payment order

**Payment Order (أمر الصرف)**:
Financial authorization to release an approved amount to a beneficiary. It may originate from a disbursement request or a procurement purchase order.
_Avoid_: Disbursement request, payment

**Payment (عملية الدفع)**:
Confirmed monetary settlement of one payment order.
_Avoid_: Payment order, disbursement request

**Payment Order Deduction (خصم أمر الصرف)**:
Amount withheld from a payment order's gross amount before settlement.
_Avoid_: Payment, gross amount
