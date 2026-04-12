import { useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { IngredientFormSection } from '@/features/catalog/IngredientFormSection';
import { useCreateIngredientMutation } from '@/lib/usePortalData';

export function CatalogIngredientCreatePage() {
  const navigate = useNavigate();
  const createIngredient = useCreateIngredientMutation();
  const [name, setName] = useState('');
  const [notableBrands, setNotableBrands] = useState('');

  async function handleSubmit() {
    await createIngredient.mutateAsync({
      name,
      notableBrands: notableBrands
        .split(',')
        .map((part) => part.trim())
        .filter(Boolean),
    });
    navigate({ to: '/catalog/ingredients' });
  }

  return (
    <CatalogShell title="Create ingredient" description="Add a new ingredient to the catalog.">
      {createIngredient.error ? (
        <InlineMessage tone="danger" message={createIngredient.error.message} />
      ) : null}

      <IngredientFormSection
        isEditing={false}
        name={name}
        notableBrands={notableBrands}
        onNameChange={setName}
        onNotableBrandsChange={setNotableBrands}
        onSubmit={handleSubmit}
        onCancel={() => navigate({ to: '/catalog/ingredients' })}
      />

      <Button asChild variant="ghost">
        <Link to="/catalog/ingredients">Back to ingredient list</Link>
      </Button>
    </CatalogShell>
  );
}
