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
import { IngredientFormSection } from '@/features/catalog/IngredientFormSection';
import {
  useDeleteIngredientMutation,
  useIngredients,
  useUpdateIngredientMutation,
} from '@/features/catalog/api/hooks';

export function CatalogIngredientEditPage() {
  const { ingredientId } = useParams({ from: '/catalog/ingredients/$ingredientId' });
  const navigate = useNavigate();
  const ingredients = useIngredients();
  const updateIngredient = useUpdateIngredientMutation();
  const deleteIngredient = useDeleteIngredientMutation();
  const [name, setName] = useState('');
  const [ingredientGroup, setIngredientGroup] = useState('');
  const [notableBrands, setNotableBrands] = useState('');
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const ingredient = (ingredients.data ?? []).find((item) => item.id === ingredientId) ?? null;

  useEffect(() => {
    if (!ingredient) return;
    setName(ingredient.name);
    setIngredientGroup(ingredient.ingredientGroup ?? '');
    setNotableBrands(ingredient.notableBrands.join(', '));
  }, [ingredient]);

  async function handleSubmit() {
    if (!ingredient) return;
    await updateIngredient.mutateAsync({
      id: ingredient.id,
      input: {
        name,
        notableBrands: notableBrands
          .split(',')
          .map((part) => part.trim())
          .filter(Boolean),
        ingredientGroup: ingredientGroup.trim() || null,
      },
    });
    navigate({ to: '/catalog/ingredients' });
  }

  function handleDelete() {
    if (!ingredient) return;
    setIsDeleteDialogOpen(true);
  }

  if (!ingredient && !ingredients.isLoading) {
    return (
      <CatalogShell title="Edit ingredient" description="This ingredient could not be found.">
        <InlineMessage tone="danger" message="Ingredient not found." />
        <Button asChild variant="ghost">
          <Link to="/catalog/ingredients">Back to ingredient list</Link>
        </Button>
      </CatalogShell>
    );
  }

  const activeError = updateIngredient.error ?? deleteIngredient.error;

  return (
    <CatalogShell
      title="Edit ingredient"
      description="Update notable brands or remove the ingredient."
    >
      {activeError ? <InlineMessage tone="danger" message={activeError.message} /> : null}

      <IngredientFormSection
        isEditing
        name={name}
        ingredientGroup={ingredientGroup}
        notableBrands={notableBrands}
        onNameChange={setName}
        onIngredientGroupChange={setIngredientGroup}
        onNotableBrandsChange={setNotableBrands}
        onSubmit={handleSubmit}
        onDelete={handleDelete}
        onCancel={() => navigate({ to: '/catalog/ingredients' })}
      />

      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete ingredient</AlertDialogTitle>
            <AlertDialogDescription>
              Delete ingredient "{name}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel asChild>
              <Button variant="outline">Cancel</Button>
            </AlertDialogCancel>
            <AlertDialogAction asChild>
              <Button
                variant="destructive"
                onClick={() => {
                  if (!ingredient) {
                    return;
                  }

                  deleteIngredient.mutate(ingredient.id, {
                    onSuccess: () => navigate({ to: '/catalog/ingredients' }),
                  });
                }}
              >
                Delete ingredient
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </CatalogShell>
  );
}
