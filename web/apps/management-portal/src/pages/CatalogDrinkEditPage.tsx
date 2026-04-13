import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from '@tanstack/react-router';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { DrinkEditorSection } from '@/features/catalog/DrinkEditorSection';
import { emptyDrinkForm, type DrinkFormState } from '@/features/catalog/types';
import {
  useDeleteDrinkMutation,
  useDrink,
  useIngredients,
  useTags,
  useUpdateDrinkMutation,
} from '@/features/catalog/useCatalogData';

export function CatalogDrinkEditPage() {
  const { drinkId } = useParams({ from: '/catalog/drinks/$drinkId' });
  const navigate = useNavigate();
  const drink = useDrink(drinkId);
  const tags = useTags();
  const ingredients = useIngredients();
  const updateDrink = useUpdateDrinkMutation();
  const deleteDrink = useDeleteDrinkMutation();
  const [drinkForm, setDrinkForm] = useState<DrinkFormState>(emptyDrinkForm);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  useEffect(() => {
    if (!drink.data) {
      return;
    }

    setDrinkForm({
      name: drink.data.name,
      category: drink.data.category ?? '',
      description: drink.data.description ?? '',
      method: drink.data.method ?? '',
      garnish: drink.data.garnish ?? '',
      imageUrl: drink.data.imageUrl ?? '',
      tagIds: drink.data.tags.map((tag) => tag.id),
      recipeEntries: drink.data.recipeEntries.map((entry) => ({
        ingredientId: entry.ingredient.id,
        quantity: entry.quantity,
        recommendedBrand: entry.recommendedBrand ?? '',
      })),
    });
  }, [drink.data]);

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

    await updateDrink.mutateAsync({ id: drinkId, input });
    navigate({ to: '/catalog/drinks' });
  }

  function handleDelete() {
    setIsDeleteDialogOpen(true);
  }

  if (drink.isLoading) {
    return (
      <CatalogShell title="Edit drink" description="Loading drink details.">
        <p className="text-sm text-muted-foreground">Loading...</p>
      </CatalogShell>
    );
  }

  if (!drink.data) {
    return (
      <CatalogShell title="Edit drink" description="This drink could not be found.">
        <InlineMessage tone="danger" message="Drink not found." />
        <Button asChild variant="ghost">
          <Link to="/catalog/drinks">Back to drink list</Link>
        </Button>
      </CatalogShell>
    );
  }

  const activeError = updateDrink.error ?? deleteDrink.error;

  return (
    <CatalogShell title="Edit drink" description="Update recipe, tags, and presentation details.">
      {activeError ? <InlineMessage tone="danger" message={activeError.message} /> : null}

      <DrinkEditorSection
        isEditing
        drinkForm={drinkForm}
        tags={tags.data ?? []}
        ingredients={(ingredients.data ?? []).map((ingredient) => ({
          id: ingredient.id,
          name: ingredient.name,
        }))}
        onDrinkFormChange={(updater) => setDrinkForm(updater)}
        onSubmit={handleSubmit}
        onDelete={handleDelete}
        onCancel={() => navigate({ to: '/catalog/drinks' })}
      />

      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete drink</AlertDialogTitle>
            <AlertDialogDescription>
              Delete drink "{drinkForm.name}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel asChild>
              <Button variant="outline">Cancel</Button>
            </AlertDialogCancel>
            <AlertDialogAction asChild>
              <Button
                variant="destructive"
                onClick={() =>
                  deleteDrink.mutate(drinkId, {
                    onSuccess: () => navigate({ to: '/catalog/drinks' }),
                  })
                }
              >
                Delete drink
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </CatalogShell>
  );
}
