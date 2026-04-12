import { useMemo } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { TagDto } from '@alcopilot/management-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { useDeleteTagMutation, useTags } from '@/lib/usePortalData';

export function CatalogTagsPage() {
  const navigate = useNavigate();
  const tags = useTags();
  const deleteTagMutation = useDeleteTagMutation();

  const columns = useMemo<ColumnDef<TagDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-slate-950">{row.original.name}</span>,
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
              if (confirm(`Delete tag "${row.original.name}"?`)) {
                deleteTagMutation.mutate(row.original.id);
              }
            }}
          >
            Delete
          </Button>
        ),
      },
    ],
    [deleteTagMutation],
  );

  return (
    <CatalogShell
      title="Tag list"
      description="Review the tag vocabulary, then open a dedicated form to create or edit a tag."
    >
      {deleteTagMutation.error ? (
        <InlineMessage tone="danger" message={deleteTagMutation.error.message} />
      ) : null}

      <SectionCard title="Tags" description="Search, sort, and open a tag record to edit.">
        <DataTable
          columns={columns}
          data={tags.data ?? []}
          searchPlaceholder="Search tags"
          toolbarAction={
            <Button asChild>
              <Link to="/catalog/tags/new">New</Link>
            </Button>
          }
          onRowClick={(row) => navigate({ to: '/catalog/tags/$tagId', params: { tagId: row.id } })}
        />
      </SectionCard>
    </CatalogShell>
  );
}
