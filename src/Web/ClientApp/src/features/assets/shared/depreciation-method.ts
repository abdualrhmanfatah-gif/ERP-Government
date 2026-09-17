export const depreciationMethodLabels: Record<string, string> = {
  StraightLine: 'القسط الثابت',
  DecliningBalance: 'القيمة المتراجحة',
  UnitsOfProduction: 'وحدات الإنتاج',
};

export const depreciationMethodOptions = Object.entries(depreciationMethodLabels).map(([value, label]) => ({ value, label }));

export function getDepreciationMethodLabel(method: string | null | undefined): string {
  if (!method) return '—';
  return depreciationMethodLabels[method] ?? method;
}
