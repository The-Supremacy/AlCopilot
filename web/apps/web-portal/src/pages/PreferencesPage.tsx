import { useEffect, useState } from 'react';
import type { CustomerProfileDto } from '@alcopilot/customer-api-client';
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

type PreferenceState = Pick<
  CustomerProfileDto,
  'favoriteIngredientIds' | 'dislikedIngredientIds' | 'prohibitedIngredientIds'
>;

export function PreferencesPage() {
  const ingredientsQuery = useCustomerIngredients();
  const profileQuery = useCustomerProfile();
  const saveMutation = useSaveCustomerProfileMutation();
  const [preferenceState, setPreferenceState] = useState<PreferenceState>({
    favoriteIngredientIds: [],
    dislikedIngredientIds: [],
    prohibitedIngredientIds: [],
  });

  useEffect(() => {
    if (profileQuery.data) {
      setPreferenceState({
        favoriteIngredientIds: profileQuery.data.favoriteIngredientIds,
        dislikedIngredientIds: profileQuery.data.dislikedIngredientIds,
        prohibitedIngredientIds: profileQuery.data.prohibitedIngredientIds,
      });
    }
  }, [profileQuery.data]);

  async function handleSave() {
    if (!profileQuery.data) {
      return;
    }

    try {
      await saveMutation.mutateAsync({
        ...profileQuery.data,
        ...preferenceState,
      });
      toast.success('Preferences saved.');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Could not save your preferences.');
    }
  }

  return (
    <div className="space-y-5">
      <SectionCard
        title="Preferences"
        description="Shape chat results with favorite ingredients, soft dislikes, and hard prohibitions."
        action={
          <Button
            className="w-full sm:w-auto"
            onClick={handleSave}
            loading={saveMutation.isPending}
            loadingText="Saving"
          >
            Save preferences
          </Button>
        }
      >
        {ingredientsQuery.isError || profileQuery.isError ? (
          <InlineMessage
            tone="danger"
            message="The portal could not load ingredient or preference data right now."
          />
        ) : null}

        {ingredientsQuery.data ? (
          <div className="space-y-6">
            <ProfileIngredientPicker
              title="Favorite ingredients"
              description="These ingredients can nudge drinks higher when they already fit the request."
              ingredients={ingredientsQuery.data}
              selectedIds={preferenceState.favoriteIngredientIds}
              onChange={(favoriteIngredientIds) =>
                setPreferenceState((current) => ({ ...current, favoriteIngredientIds }))
              }
              emptySelectionMessage="No favorites selected yet. Chat will rely on your prompt and the rest of your profile."
            />

            <ProfileIngredientPicker
              title="Disliked ingredients"
              description="Dislikes are a soft penalty, not an absolute block."
              ingredients={ingredientsQuery.data}
              selectedIds={preferenceState.dislikedIngredientIds}
              onChange={(dislikedIngredientIds) =>
                setPreferenceState((current) => ({ ...current, dislikedIngredientIds }))
              }
              emptySelectionMessage="No dislikes selected yet."
            />

            <ProfileIngredientPicker
              title="Prohibited ingredients"
              description="Anything here is treated as a hard exclusion before scoring."
              ingredients={ingredientsQuery.data}
              selectedIds={preferenceState.prohibitedIngredientIds}
              onChange={(prohibitedIngredientIds) =>
                setPreferenceState((current) => ({ ...current, prohibitedIngredientIds }))
              }
              emptySelectionMessage="No prohibited ingredients selected yet."
            />
          </div>
        ) : null}
      </SectionCard>
    </div>
  );
}
