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
import { TagFormSection } from '@/features/catalog/TagFormSection';
import { useDeleteTagMutation, useTags, useUpdateTagMutation } from '@/features/catalog/api/hooks';

export function CatalogTagEditPage() {
  const { tagId } = useParams({ from: '/catalog/tags/$tagId' });
  const navigate = useNavigate();
  const tags = useTags();
  const updateTag = useUpdateTagMutation();
  const deleteTag = useDeleteTagMutation();
  const [name, setName] = useState('');
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

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
    setIsDeleteDialogOpen(true);
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

      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete tag</AlertDialogTitle>
            <AlertDialogDescription>
              Delete tag "{name}"? This action cannot be undone.
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
                  if (!tag) {
                    return;
                  }

                  deleteTag.mutate(tag.id, { onSuccess: () => navigate({ to: '/catalog/tags' }) });
                }}
              >
                Delete tag
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </CatalogShell>
  );
}
