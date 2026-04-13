import { useMemo, useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { DrinkDto } from '@alcopilot/management-api-client';
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
import { useDeleteDrinkMutation, useDrinks } from '@/features/catalog/useCatalogData';
import { joinLines } from '@/lib/format';

export function CatalogDrinksPage() {
  const navigate = useNavigate();
  const drinks = useDrinks();
  const deleteDrinkMutation = useDeleteDrinkMutation();
  const [drinkPendingDelete, setDrinkPendingDelete] = useState<DrinkDto | null>(null);

  const columns = useMemo<ColumnDef<DrinkDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-foreground">{row.original.name}</span>,
      },
      {
        accessorKey: 'category',
        header: 'Category',
        cell: ({ row }) => row.original.category || 'Unassigned',
      },
      {
        accessorKey: 'method',
        header: 'Method',
        cell: ({ row }) => row.original.method || '—',
      },
      {
        accessorKey: 'garnish',
        header: 'Garnish',
        cell: ({ row }) => row.original.garnish || '—',
      },
      {
        id: 'tags',
        header: 'Tags',
        cell: ({ row }) => joinLines(row.original.tags.map((tag) => tag.name)) || '—',
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
              setDrinkPendingDelete(row.original);
            }}
          >
            Delete
          </Button>
        ),
      },
    ],
    [deleteDrinkMutation],
  );

  return (
    <CatalogShell
      title="Drink list"
      description="Browse drinks first, then use the separate form surface to create or edit a selected drink."
    >
      {deleteDrinkMutation.error ? (
        <InlineMessage tone="danger" message={deleteDrinkMutation.error.message} />
      ) : null}

      <SectionCard title="Drinks" description="Search, sort, and open a drink record to edit.">
        <DataTable
          columns={columns}
          data={drinks.data?.items ?? []}
          searchPlaceholder="Search drinks"
          getRowAriaLabel={(row) => `Open drink ${row.name}`}
          toolbarAction={
            <Button asChild>
              <Link to="/catalog/drinks/new">New</Link>
            </Button>
          }
          onRowClick={(row) =>
            navigate({ to: '/catalog/drinks/$drinkId', params: { drinkId: row.id } })
          }
        />
      </SectionCard>

      <AlertDialog
        open={drinkPendingDelete !== null}
        onOpenChange={(open: boolean) => {
          if (!open) {
            setDrinkPendingDelete(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete drink</AlertDialogTitle>
            <AlertDialogDescription>
              {drinkPendingDelete
                ? `Delete drink "${drinkPendingDelete.name}"? This action cannot be undone.`
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
                  if (!drinkPendingDelete) {
                    return;
                  }

                  deleteDrinkMutation.mutate(drinkPendingDelete.id, {
                    onSuccess: () => setDrinkPendingDelete(null),
                  });
                }}
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
