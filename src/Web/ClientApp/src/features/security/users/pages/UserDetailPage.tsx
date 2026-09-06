import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { notify } from '@/features/notifications/notify';
import { Pencil, Save, X, Shield, Lock, Users as UsersIcon } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { FormField } from '@/components/ui/FormField';
import { Tabs } from '@/components/ui/Tabs';
import { Skeleton } from '@/components/ui/Loading';
import { useUserDetail, useUpdateUser } from '../hooks';
import { useOrganizationalUnits } from '../../../organization/hooks';
import { RolesTab } from '../components/RolesTab';
import { PermissionsTab } from '../components/PermissionsTab';
import { SessionsTab } from '../components/SessionsTab';
import { AuditTab } from '../components/AuditTab';
import { DeactivateUserDialog } from '../components/DeactivateUserDialog';
import { ReactivateUserDialog } from '../components/ReactivateUserDialog';

export function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const userId = parseInt(id ?? '0', 10);
  const { data: user, isLoading, error } = useUserDetail(userId);
  const updateUser = useUpdateUser();
  const { data: departments = [] } = useOrganizationalUnits();

  const [isEditing, setIsEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editDepartmentId, setEditDepartmentId] = useState('');
  const [editAccountType, setEditAccountType] = useState('');
  const [showDeactivate, setShowDeactivate] = useState(false);
  const [showReactivate, setShowReactivate] = useState(false);

  function startEdit() {
    if (!user) return;
    setEditName(user.login);
    setEditDepartmentId(user.departmentId?.toString() ?? '');
    setEditAccountType(user.accountType);
    setIsEditing(true);
  }

  async function handleSave() {
    if (!user) return;
    try {
      await updateUser.mutateAsync({
        id: user.id,
        name: editName,
        departmentId: editDepartmentId ? Number(editDepartmentId) : undefined,
        accountType: editAccountType,
        rowVersion: (user as any).rowVersion ?? '',
      });
      notify({ type: 'success', title: 'تم الحفظ بنجاح' });
      setIsEditing(false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء الحفظ';
      if (message.includes('409') || message.includes('Conflict')) {
        notify({ type: 'error', title: 'تعارض في البيانات — يرجى تحديث الصفحة' });
      } else {
        notify({ type: 'error', title: message });
      }
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton variant="heading" className="w-1/3" />
        <Skeleton variant="card" />
        <Skeleton variant="table" lines={4} />
      </div>
    );
  }

  if (error || !user) {
    return <div className="p-8 text-center text-[var(--color-on-surface-variant)]">المستخدم غير موجود</div>;
  }

  const tabs = [
    { key: 'roles', label: 'الأدوار', icon: <UsersIcon size={16} />, content: <RolesTab userId={userId} /> },
    { key: 'permissions', label: 'الصلاحيات', icon: <Shield size={16} />, content: <PermissionsTab userId={userId} /> },
    { key: 'sessions', label: 'الجلسات', icon: <Lock size={16} />, content: <SessionsTab userId={userId} /> },
    { key: 'audit', label: 'سجل التغييرات', icon: <Pencil size={16} />, content: <AuditTab userId={userId} /> },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={user.login}
        description={isEditing ? 'تعديل بيانات المستخدم' : `تفاصيل المستخدم — ${user.departmentName ?? '—'}`}
        actions={
          <div className="flex gap-2">
            {user.isActive ? (
              <Button variant="ghost" size="sm" onClick={() => setShowDeactivate(true)}>
                تعطيل
              </Button>
            ) : (
              <Button variant="ghost" size="sm" onClick={() => setShowReactivate(true)}>
                تنشيط
              </Button>
            )}
            {isEditing ? (
              <>
                <Button variant="primary" size="sm" icon={<Save size={14} />} onClick={handleSave} loading={updateUser.isPending}>
                  حفظ
                </Button>
                <Button variant="ghost" size="sm" icon={<X size={14} />} onClick={() => setIsEditing(false)}>
                  إلغاء
                </Button>
              </>
            ) : (
              <Button variant="ghost" size="sm" icon={<Pencil size={14} />} onClick={startEdit}>
                تعديل
              </Button>
            )}
          </div>
        }
      />

      <div className="flex flex-wrap gap-3 items-center">
        <StatusBadge variant={user.isActive ? 'active' : 'draft'}>
          {user.isActive ? 'نشط' : 'غير نشط'}
        </StatusBadge>
        {user.mfaEnabled && (
          <StatusBadge variant="active">المصادقة الثنائية مفعّلة</StatusBadge>
        )}
        {user.isLocked && (
          <StatusBadge variant="draft">مقفل</StatusBadge>
        )}
        <span className="text-sm text-[var(--color-on-surface-variant)] me-4">
          جلسات نشطة: {user.activeSessionCount}
        </span>
      </div>

      {isEditing ? (
        <div className="max-w-xl space-y-4">
          <FormField label="الاسم">
            <Input value={editName} onChange={(e) => setEditName(e.target.value)} />
          </FormField>
          <Select
            label="القسم"
            value={editDepartmentId}
            onChange={(e) => setEditDepartmentId(e.target.value)}
            options={[{ value: '', label: '— اختر القسم —' }, ...departments.map((d) => ({ value: String(d.id), label: d.name }))]}
          />
          <Select
            label="نوع الحساب"
            value={editAccountType}
            onChange={(e) => setEditAccountType(e.target.value)}
            options={[
              { value: 'Internal', label: 'داخلي' },
              { value: 'External', label: 'خارجي' },
            ]}
          />
        </div>
      ) : (
        <div className="bg-[var(--color-surface)] rounded-lg border border-[var(--color-border-container)] p-4">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">الاسم</span>
              <span className="font-medium">{user.login}</span>
            </div>
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">القسم</span>
              <span className="font-medium">{user.departmentName ?? '—'}</span>
            </div>
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">نوع الحساب</span>
              <span className="font-medium">{user.accountType}</span>
            </div>
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">آخر دخول</span>
              <span className="font-medium">{user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString('ar') : '—'}</span>
            </div>
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">تاريخ الإنشاء</span>
              <span className="font-medium">{new Date(user.createdAt).toLocaleDateString('ar')}</span>
            </div>
            <div className="space-y-1">
              <span className="text-[var(--color-on-surface-variant)] block">أنشأه</span>
              <span className="font-medium">{user.createdBy ?? '—'}</span>
            </div>
          </div>
        </div>
      )}

      <Tabs tabs={tabs} />

      <DeactivateUserDialog open={showDeactivate} onClose={() => setShowDeactivate(false)} userId={userId} userName={user.login} />
      <ReactivateUserDialog open={showReactivate} onClose={() => setShowReactivate(false)} userId={userId} userName={user.login} />
    </div>
  );
}
