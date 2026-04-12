import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { TagFormSection } from '@/features/catalog/TagFormSection';
import { useDeleteTagMutation, useTags, useUpdateTagMutation } from '@/lib/usePortalData';

export function CatalogTagEditPage() {
  const { tagId } = useParams({ from: '/catalog/tags/$tagId' });
  const navigate = useNavigate();
  const tags = useTags();
  const updateTag = useUpdateTagMutation();
  const deleteTag = useDeleteTagMutation();
  const [name, setName] = useState('');

  const tag = (tags.data ?? []).find((item) => item.id === tagId) ?? null;

  useEffect(() => {
    if (tag) {
      setName(tag.name);
    }
  }, [tag]);

  async function handleSubmit() {
    if (!tag) return;
    await updateTag.mutateAsync({ id: tag.id, input: { name } });
    navigate({ to: '/catalog/tags' });
  }

  function handleDelete() {
    if (!tag) return;
    if (confirm(`Delete tag "${tag.name}"?`)) {
      deleteTag.mutate(tag.id, { onSuccess: () => navigate({ to: '/catalog/tags' }) });
    }
  }

  if (!tag && !tags.isLoading) {
    return (
      <CatalogShell title="Edit tag" description="This tag could not be found.">
        <InlineMessage tone="danger" message="Tag not found." />
        <Button asChild variant="ghost">
          <Link to="/catalog/tags">Back to tag list</Link>
        </Button>
      </CatalogShell>
    );
  }

  const activeError = updateTag.error ?? deleteTag.error;

  return (
    <CatalogShell title="Edit tag" description="Update tag name or remove it from the catalog.">
      {activeError ? <InlineMessage tone="danger" message={activeError.message} /> : null}

      <TagFormSection
        isEditing
        name={name}
        onNameChange={setName}
        onSubmit={handleSubmit}
        onDelete={handleDelete}
        onCancel={() => navigate({ to: '/catalog/tags' })}
      />
    </CatalogShell>
  );
}
