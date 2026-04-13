import { useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { TagFormSection } from '@/features/catalog/TagFormSection';
import { useCreateTagMutation } from '@/features/catalog/useCatalogData';

export function CatalogTagCreatePage() {
  const navigate = useNavigate();
  const createTag = useCreateTagMutation();
  const [name, setName] = useState('');

  async function handleSubmit() {
    await createTag.mutateAsync({ name });
    navigate({ to: '/catalog/tags' });
  }

  return (
    <CatalogShell title="Create tag" description="Create a new tag in a focused form.">
      {createTag.error ? <InlineMessage tone="danger" message={createTag.error.message} /> : null}

      <TagFormSection
        isEditing={false}
        name={name}
        onNameChange={setName}
        onSubmit={handleSubmit}
        onCancel={() => navigate({ to: '/catalog/tags' })}
      />

      <Button asChild variant="ghost">
        <Link to="/catalog/tags">Back to tag list</Link>
      </Button>
    </CatalogShell>
  );
}
