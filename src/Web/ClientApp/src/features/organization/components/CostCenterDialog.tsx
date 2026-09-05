import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { CostCenterForm } from './CostCenterForm';
import type { CostCenterDto, CreateCostCenterCommand } from '../types';

interface CostCenterDialogProps {
  open: boolean;
  onClose: () => void;
  initialData?: CostCenterDto;
  isEdit?: boolean;
  onSubmit: (data: CreateCostCenterCommand) => void;
  serverError?: string;
  loading?: boolean;
}

export function CostCenterDialog({
  open,
  onClose,
  initialData,
  isEdit,
  onSubmit,
  serverError,
  loading,
}: CostCenterDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={isEdit ? 'تعديل مركز التكلفة' : 'إضافة مركز تكلفة جديد'}
      footer={
        <>
          <Button variant="ghost" onClick={onClose} disabled={loading}>
            إلغاء
          </Button>
          <Button type="submit" variant="primary" loading={loading} form="cost-center-form">
            {isEdit ? 'تحديث' : 'إنشاء'}
          </Button>
        </>
      }
    >
      <CostCenterForm
        id="cost-center-form"
        initialData={initialData}
        isEdit={isEdit}
        onSubmit={onSubmit}
        serverError={serverError}
        loading={loading}
      />
    </Dialog>
  );
}
