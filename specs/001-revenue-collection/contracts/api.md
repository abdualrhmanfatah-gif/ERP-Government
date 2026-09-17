# عقود الواجهة: نظام إدارة وتوريد الإيرادات الحكومية

## عقود API endpoints

### RevenueClaims

#### POST /api/RevenueClaims
```json
{
  "claimDate": "2026-09-13",
  "partyId": 1,
  "totalAmount": 50000.00
}
```
**Response**: `201 Created`
```json
{
  "id": 1,
  "claimNumber": "RC-000001",
  "claimDate": "2026-09-13",
  "partyId": 1,
  "partyName": "...",
  "totalAmount": 50000.00,
  "status": "Draft",
  "outstandingAmount": 50000.00,
  "collectedAmount": 0,
  "availableAmount": 50000.00
}
```

#### POST /api/RevenueClaims/{id}/approve
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result

---

### CollectionOrders

#### POST /api/CollectionOrders
```json
{
  "revenueClaimId": 1,
  "orderDate": "2026-09-13",
  "authorizedAmount": 50000.00
}
```
**Response**: `201 Created`
```json
{
  "id": 1,
  "orderNumber": "CO-000001",
  "revenueClaimId": 1,
  "orderDate": "2026-09-13",
  "authorizedAmount": 50000.00,
  "status": "Draft"
}
```

#### POST /api/CollectionOrders/{id}/approve
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result

---

### ReceiptVouchers

#### POST /api/ReceiptVouchers
```json
{
  "collectionOrderId": 1,
  "voucherDate": "2026-09-13",
  "paymentMethod": "Cash",
  "receivedFrom": "أحمد محمد",
  "lines": [
    {
      "revenueAccountId": 10,
      "amount": 25000.00,
      "description": "دفعة أولى"
    }
  ]
}
```
**Response**: `201 Created`
```json
{
  "id": 1,
  "voucherNumber": "RV-000001",
  "collectionOrderId": 1,
  "voucherDate": "2026-09-13",
  "paymentMethod": "Cash",
  "receivedFrom": "أحمد محمد",
  "status": "Draft",
  "totalAmount": 25000.00,
  "lines": [...]
}
```

#### POST /api/ReceiptVouchers/{id}/approve
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result
- يُنشئ قيد محاسبي تلقائيًا
- يُحدّث حالة المطالبة

---

### Checks

#### POST /api/Checks/{id}/clear
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result
- يُنشئ قيدين محاسبيين
- يُحدّح حالة الشيك إلى Cleared
- يُحدّح حالة المطالبة

#### POST /api/Checks/{id}/bounce
```json
{
  "id": 1,
  "rowVersion": "...",
  "reason": "أرصدة غير كافية"
}
```
**Response**: `200 OK` with Result
- يُنشئ قيد عكس
- يُحدّح حالة الشيك إلى Bounced

---

### DepositSlips

#### POST /api/DepositSlips/slip47
```json
{
  "slipDate": "2026-09-13",
  "receiptVoucherIds": [1, 2, 3]
}
```
**Response**: `201 Created`

#### POST /api/DepositSlips/slip47/{id}/approve
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result

#### POST /api/DepositSlips/slip48
```json
{
  "slipDate": "2026-09-13",
  "checkIds": [1, 2, 3]
}
```
**Response**: `201 Created`

#### POST /api/DepositSlips/slip48/{id}/approve
```json
{
  "id": 1,
  "rowVersion": "..."
}
```
**Response**: `200 OK` with Result

---

## أكواد الخطأ

| الرمز | الوصف |
|-------|-------|
| 400 | بيانات غير صحيحة |
| 404 | الكيان غير موجود |
| 409 | تعارض في الحالة (ال Kranken ordered) |
| 422 | انتهاك قاعدة العمل (تحصيل زائد) |
