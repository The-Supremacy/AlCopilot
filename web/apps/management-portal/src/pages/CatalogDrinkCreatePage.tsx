import { useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { DrinkEditorSection } from '@/features/catalog/DrinkEditorSection';
import { useCreateDrinkMutation, useIngredients, useTags } from '@/features/catalog/useCatalogData';
import { emptyDrinkForm, type DrinkFormState } from '@/features/catalog/types';

export function CatalogDrinkCreatePage() {
  const navigate = useNavigate();
  const tags = useTags();
  const ingredients = useIngredients();
  const createDrink = useCreateDrinkMutation();
  const [drinkForm, setDrinkForm] = useState<DrinkFormState>(emptyDrinkForm);

  async function handleSubmit() {
    const input = {
      name: drinkForm.name,
      category: drinkForm.category || null,
      description: drinkForm.description || null,
      method: drinkForm.method || null,
      garnish: drinkForm.garnish || null,
      imageUrl: drinkForm.imageUrl || null,
      tagIds: drinkForm.tagIds,
      recipeEntries: drinkForm.recipeEntries
        .filter((entry) => entry.ingredientId && entry.quantity)
        .map((entry) => ({
          ingredientId: entry.ingredientId,
          quantity: entry.quantity,
          recommendedBrand: entry.recommendedBrand || null,
        })),
    };

    await createDrink.mutateAsync(input);
    navigate({ to: '/catalog/drinks' });
  }

  return (
    <CatalogShell
      title="Create drink"
      description="Fill out recipe, tags, and presentation details before saving a new drink."
    >
      {createDrink.error ? (
        <InlineMessage tone="danger" message={createDrink.error.message} />
      ) : null}

      <DrinkEditorSection
        isEditing={false}
        drinkForm={drinkForm}
        tags={tags.data ?? []}
        ingredients={(ingredients.data ?? []).map((ingredient) => ({
          id: ingredient.id,
          name: ingredient.name,
        }))}
        onDrinkFormChange={(updater) => setDrinkForm(updater)}
        onSubmit={handleSubmit}
        onCancel={() => navigate({ to: '/catalog/drinks' })}
      />

      <Button asChild variant="ghost">
        <Link to="/catalog/drinks">Back to drink list</Link>
      </Button>
    </CatalogShell>
  );
}
