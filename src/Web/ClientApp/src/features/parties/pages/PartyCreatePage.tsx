import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateParty } from '../hooks/useParties';
import { usePermission } from '@/shared/hooks/usePermission';
import { PartyForm } from '../components/PartyForm';
import type { CreatePartyCommand } from '../shared/types';

export default function PartyCreatePage() {
  const navigate = useNavigate();
  const createParty = useCreateParty();
  const { hasPermission: canCreate } = usePermission('Parties.Create');

  if (!canCreate) {
    return (
      <div className="p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">غير مصرح بالوصول</p>
      </div>
    );
  }

  function onSubmit(data: CreatePartyCommand) {
    createParty.mutate(data, {
      onSuccess: (id) => navigate(`/parties/${id}`),
      onError: () => {},
    });
  }

  return (
    <Page title="مورد جديد">
      <PartyForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/parties')}
        isPending={createParty.isPending}
      />
    </Page>
  );
}
