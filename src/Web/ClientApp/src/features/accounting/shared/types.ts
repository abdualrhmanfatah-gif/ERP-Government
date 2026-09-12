import { MoveEntryType } from '../../../web-api-client';

export const statusFilters = [
  { key: '', label: 'الكل' },
  { key: 'Draft', label: 'مسودة' },
  { key: 'Submitted', label: 'مرسل للمراجعة' },
  { key: 'Approved', label: 'موافق عليه' },
  { key: 'Posted', label: 'مرحل' },
  { key: 'Reversed', label: 'معكوس' },
  { key: 'Cancelled', label: 'ملغى' },
];

export const entryTypeLabels: Record<MoveEntryType, string> = {
  [MoveEntryType.Standard]: 'قيود عامة',
  [MoveEntryType.Reversing]: 'قيود عكسية',
  [MoveEntryType.Adjusting]: 'قيود تسوية',
  [MoveEntryType.Opening]: 'قيد افتتاحي',
  [MoveEntryType.Closing]: 'قيد إغلاق',
  [MoveEntryType.SystemGenerated]: 'مولد آلياً',
  [MoveEntryType.Accrual]: 'قيد استحقاق',
};

export const manualEntryTypes: MoveEntryType[] = [
  MoveEntryType.Standard,
  MoveEntryType.Reversing,
  MoveEntryType.Adjusting,
  MoveEntryType.Opening,
  MoveEntryType.Accrual,
];
