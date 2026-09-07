import { useState } from 'react';
import { notify } from '@/features/notifications/notify';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { FormField } from '@/components/ui/FormField';
import { useCreateUser } from '@/features/security/users/hooks';
import { useRoles } from '@/features/security/rbac/hooks';
import { useOrganizationalUnits } from '@/features/organization/hooks';

interface CreateUserDialogProps {
  open: boolean;
  onClose: () => void;
  onCreated: (id: number) => void;
}

export function CreateUserDialog({ open, onClose, onCreated }: CreateUserDialogProps) {
  const createUser = useCreateUser();
  const { data: roles = [] } = useRoles();
  const { data: departments = [] } = useOrganizationalUnits();

  const [login, setLogin] = useState('');
  const [name, setName] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [accountType, setAccountType] = useState('Internal');
  const [roleId, setRoleId] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  function reset() {
    setLogin(''); setName(''); setDepartmentId(''); setAccountType('Internal');
    setRoleId(''); setPassword(''); setConfirmPassword(''); setErrors({});
  }

  function handleClose() { reset(); onClose(); }

  function validate(): boolean {
    const errs: Record<string, string> = {};
    if (!login.trim()) errs.login = 'مطلوب';
    if (!name.trim()) errs.name = 'مطلوب';
    if (!password) errs.password = 'مطلوب';
    if (password.length < 8) errs.password = 'يجب أن تكون 8 أحرف على الأقل';
    if (password !== confirmPassword) errs.confirmPassword = 'كلمتا المرور غير متطابقتين';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    try {
      const id = await createUser.mutateAsync({
        login: login.trim(),
        name: name.trim(),
        departmentId: departmentId ? Number(departmentId) : undefined,
        accountType,
        password,
      });
      notify({ type: 'success', title: 'تم إنشاء المستخدم بنجاح' });
      handleClose();
      onCreated(id);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء المستخدم';
      notify({ type: 'error', title: message });
    }
  }

  return (
    <Dialog open={open} onClose={handleClose} title="مستخدم جديد"
      footer={<>
        <Button variant="ghost" onClick={handleClose}>إلغاء</Button>
        <Button variant="primary" loading={createUser.isPending} onClick={handleSubmit}>إنشاء</Button>
      </>}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="اسم المستخدم" error={errors.login}>
          <Input value={login} onChange={(e) => setLogin(e.target.value)} placeholder="اسم المستخدم" />
        </FormField>
        <FormField label="الاسم" error={errors.name}>
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="الاسم الكامل" />
        </FormField>
        <Select label="القسم" value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}
          options={[{ value: '', label: '— اختر القسم —' }, ...departments.map((d) => ({ value: String(d.id), label: d.name }))]} />
        <Select label="نوع الحساب" value={accountType} onChange={(e) => setAccountType(e.target.value)}
          options={[{ value: 'Internal', label: 'داخلي' }, { value: 'External', label: 'خارجي' }]} />
        <Select label="الدور" value={roleId} onChange={(e) => setRoleId(e.target.value)}
          options={[{ value: '', label: '— بدون دور —' }, ...roles.map((r) => ({ value: String(r.id), label: r.name }))]} />
        <FormField label="كلمة المرور" error={errors.password}>
          <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </FormField>
        <FormField label="تأكيد كلمة المرور" error={errors.confirmPassword}>
          <Input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} />
        </FormField>
      </form>
    </Dialog>
  );
}
