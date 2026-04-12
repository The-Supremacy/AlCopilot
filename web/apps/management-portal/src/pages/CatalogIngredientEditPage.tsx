import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { IngredientFormSection } from '@/features/catalog/IngredientFormSection';
import {
  useDeleteIngredientMutation,
  useIngredients,
  useUpdateIngredientMutation,
} from '@/lib/usePortalData';

export function CatalogIngredientEditPage() {
  const { ingredientId } = useParams({ from: '/catalog/ingredients/$ingredientId' });
  const navigate = useNavigate();
  const ingredients = useIngredients();
  const updateIngredient = useUpdateIngredientMutation();
  const deleteIngredient = useDeleteIngredientMutation();
  const [name, setName] = useState('');
  const [notableBrands, setNotableBrands] = useState('');

  const ingredient = (ingredients.data ?? []).find((item) => item.id === ingredientId) ?? null;

  useEffect(() => {
    if (!ingredient) return;
    setName(ingredient.name);
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
      },
    });
    navigate({ to: '/catalog/ingredients' });
  }

  function handleDelete() {
    if (!ingredient) return;
    if (confirm(`Delete ingredient "${ingredient.name}"?`)) {
      deleteIngredient.mutate(ingredient.id, {
        onSuccess: () => navigate({ to: '/catalog/ingredients' }),
      });
    }
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
        notableBrands={notableBrands}
        onNameChange={setName}
        onNotableBrandsChange={setNotableBrands}
        onSubmit={handleSubmit}
        onDelete={handleDelete}
        onCancel={() => navigate({ to: '/catalog/ingredients' })}
      />
    </CatalogShell>
  );
}
