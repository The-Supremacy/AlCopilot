import { useEffect, useState } from 'react';
import { toast } from 'sonner';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import {
  useCustomerIngredients,
  useCustomerProfile,
  useSaveCustomerProfileMutation,
} from '@/features/profile/hooks';
import { ProfileIngredientPicker } from '@/features/profile/ProfileIngredientPicker';

export function MyBarPage() {
  const ingredientsQuery = useCustomerIngredients();
  const profileQuery = useCustomerProfile();
  const saveMutation = useSaveCustomerProfileMutation();
  const [ownedIngredientIds, setOwnedIngredientIds] = useState<string[]>([]);

  useEffect(() => {
    if (profileQuery.data) {
      setOwnedIngredientIds(profileQuery.data.ownedIngredientIds);
    }
  }, [profileQuery.data]);

  async function handleSave() {
    if (!profileQuery.data) {
      return;
    }

    try {
      await saveMutation.mutateAsync({
        ...profileQuery.data,
        ownedIngredientIds,
      });
      toast.success('Home bar saved.');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Could not save your home bar.');
    }
  }

  return (
    <SectionCard
      title="My Bar"
      description="Track what is already on hand so chat can separate immediate pours from drinks that need a shopping run."
      action={
        <Button
          className="w-full sm:w-auto"
          onClick={handleSave}
          loading={saveMutation.isPending}
          loadingText="Saving"
        >
          Save my bar
        </Button>
      }
    >
      {ingredientsQuery.isError || profileQuery.isError ? (
        <InlineMessage
          tone="danger"
          message="The portal could not load ingredient or profile data for your home bar."
        />
      ) : null}

      {ingredientsQuery.data && profileQuery.data ? (
        <ProfileIngredientPicker
          title="Owned ingredients"
          description="Choose the bottles, mixers, and staples you currently have available."
          ingredients={ingredientsQuery.data}
          selectedIds={ownedIngredientIds}
          onChange={setOwnedIngredientIds}
          emptySelectionMessage="No owned ingredients selected yet. Recommendation results will skew toward buy-next picks."
        />
      ) : null}
    </SectionCard>
  );
}
