export type AvailabilityTone = 'green' | 'amber' | 'red' | 'neutral';

export function resolveAvailabilityTone(
  available: number | undefined,
  controlMethod: string | undefined,
): AvailabilityTone {
  if (available === undefined) return 'neutral';
  if (available < 0) return controlMethod === 'Blocking' ? 'red' : 'amber';
  return 'green';
}

const ARABIC_LABELS: Record<string, string> = {
  submit: 'اعتماد للإرسال',
  approve: 'اعتماد',
  activate: 'تفعيل',
  suspend: 'تعليق',
  close: 'إغلاق',
  cancel: 'إلغاء',
  release: 'إفراج',
  liquidatePartial: 'تسوية جزئية',
  liquidateFull: 'تسوية كاملة',
  reverse: 'عكس',
  update: 'تعديل',
  delete: 'حذف',
};

export function actionLabel(key: string): string {
  return ARABIC_LABELS[key] ?? key;
}
