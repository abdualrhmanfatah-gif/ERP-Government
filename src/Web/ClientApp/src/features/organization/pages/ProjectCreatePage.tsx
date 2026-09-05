import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { ProjectForm } from '../components/ProjectForm';
import { useProject, useCreateProject, useUpdateProject } from '../hooks';
import type { CreateProjectCommand } from '../types';

export function ProjectCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateProject();

  const handleSubmit = async (data: CreateProjectCommand) => {
    await createMutation.mutateAsync(data);
    navigate('/organization/projects');
  };

  return (
    <div>
      <PageHeader title="مشروع جديد" description="إضافة مشروع جديد" />
      <div className="max-w-xl">
        <ProjectForm
          onSubmit={handleSubmit}
          serverError={createMutation.error?.message}
          loading={createMutation.isPending}
        />
      </div>
    </div>
  );
}

export function ProjectEditPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const projectId = parseInt(id ?? '0', 10);
  const { data: project, isLoading } = useProject(projectId);
  const updateMutation = useUpdateProject();

  const handleSubmit = async (data: CreateProjectCommand) => {
    await updateMutation.mutateAsync({
      ...data,
      id: projectId,
      status: project?.status ?? 'Draft',
      isActive: project?.isActive ?? true,
    });
    navigate('/organization/projects');
  };

  if (isLoading) return <div className="p-4">جاري التحميل...</div>;
  if (!project) return <div className="p-4">المشروع غير موجود</div>;

  return (
    <div>
      <PageHeader title={`تعديل: ${project.name}`} description="تحديث بيانات المشروع" />
      <div className="max-w-xl">
        <ProjectForm
          initialData={project}
          isEdit
          onSubmit={handleSubmit}
          serverError={updateMutation.error?.message}
          loading={updateMutation.isPending}
        />
      </div>
    </div>
  );
}
