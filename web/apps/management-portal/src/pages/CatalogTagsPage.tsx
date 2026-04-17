import { useMemo, useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { TagDto } from '@alcopilot/management-api-client';
import { Button } from '@/components/ui/button';
import { CatalogEntityListView } from '@/features/catalog/CatalogEntityListView';
import { useDeleteTagMutation, useTags } from '@/features/catalog/api/hooks';

export function CatalogTagsPage() {
  const navigate = useNavigate();
  const tags = useTags();
  const deleteTagMutation = useDeleteTagMutation();
  const [tagPendingDelete, setTagPendingDelete] = useState<TagDto | null>(null);

  const columns = useMemo<ColumnDef<TagDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-foreground">{row.original.name}</span>,
      },
      {
        accessorKey: 'drinkCount',
        header: 'Drink count',
        cell: ({ row }) => row.original.drinkCount,
      },
      {
        id: 'actions',
        header: 'Actions',
        cell: ({ row }) => (
          <Button
            variant="ghost"
            size="sm"
            onClick={(event) => {
              event.stopPropagation();
              setTagPendingDelete(row.original);
            }}
          >
            Delete
          </Button>
        ),
      },
    ],
    [],
  );

  return (
    <CatalogEntityListView
      title="Tag list"
      description="Review the tag vocabulary, then open a dedicated form to create or edit a tag."
      sectionTitle="Tags"
      sectionDescription="Search, sort, and open a tag record to edit."
      data={tags.data ?? []}
      columns={columns}
      searchPlaceholder="Search tags"
      newTo="/catalog/tags/new"
      getRowAriaLabel={(row) => `Open tag ${row.name}`}
      renderMobileCard={(row) => (
        <div className="space-y-4">
          <div className="space-y-1">
            <p className="text-lg font-semibold text-foreground">{row.name}</p>
            <p className="text-sm text-muted-foreground">
              {row.drinkCount} linked drink{row.drinkCount === 1 ? '' : 's'}
            </p>
          </div>
          <div className="flex justify-end">
            <Button
              variant="ghost"
              size="sm"
              onClick={(event) => {
                event.stopPropagation();
                setTagPendingDelete(row);
              }}
            >
              Delete
            </Button>
          </div>
        </div>
      )}
      onRowClick={(row) => navigate({ to: '/catalog/tags/$tagId', params: { tagId: row.id } })}
      errorMessage={deleteTagMutation.error?.message ?? null}
      pendingDelete={tagPendingDelete}
      setPendingDelete={setTagPendingDelete}
      confirmDeleteTitle="Delete tag"
      getConfirmDeleteDescription={(row) =>
        `Delete tag "${row.name}"? This action cannot be undone.`
      }
      confirmDeleteLabel="Delete tag"
      onConfirmDelete={(row) =>
        deleteTagMutation.mutate(row.id, {
          onSuccess: () => setTagPendingDelete(null),
        })
      }
    />
  );
}
