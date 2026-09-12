export const statusMap: Record<string, { label: string; variant: 'approved' | 'pending' | 'draft' }> = {
  posted: { label: 'مرحل', variant: 'approved' },
  pending: { label: 'معلق', variant: 'pending' },
  draft: { label: 'مسودة', variant: 'draft' },
};
