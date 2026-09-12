import { MoveEntryType } from '../../../web-api-client';

export const statusFilters = [
  { key: '', label: 'الكل' },
  { key: 'Draft', label: 'مسودة' },
  { key: 'Submitted', label: 'مرسل للمراجعة' },
  { key: 'Approved', label: 'موافق عليه' },
  { key: 'Posted', label: 'مسجل' },
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
};

export const manualEntryTypes: MoveEntryType[] = [
  MoveEntryType.Standard,
  MoveEntryType.Reversing,
  MoveEntryType.Adjusting,
  MoveEntryType.Opening,
];
