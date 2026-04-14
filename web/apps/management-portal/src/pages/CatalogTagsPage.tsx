import { useMemo, useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { TagDto } from '@alcopilot/management-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
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
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { CatalogShell } from '@/features/catalog/CatalogShell';
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
          toolbarAction={
            <Button asChild>
              <Link to="/catalog/tags/new">New</Link>
            </Button>
          }
          onRowClick={(row) => navigate({ to: '/catalog/tags/$tagId', params: { tagId: row.id } })}
        />
      </SectionCard>

      <AlertDialog
        open={tagPendingDelete !== null}
        onOpenChange={(open: boolean) => {
          if (!open) {
            setTagPendingDelete(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete tag</AlertDialogTitle>
            <AlertDialogDescription>
              {tagPendingDelete
                ? `Delete tag "${tagPendingDelete.name}"? This action cannot be undone.`
                : ''}
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
                  if (!tagPendingDelete) {
                    return;
                  }

                  deleteTagMutation.mutate(tagPendingDelete.id, {
                    onSuccess: () => setTagPendingDelete(null),
                  });
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
