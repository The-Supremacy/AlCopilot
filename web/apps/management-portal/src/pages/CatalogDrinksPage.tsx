import { useMemo, useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { DrinkDto } from '@alcopilot/management-api-client';
import { Button } from '@/components/ui/button';
import { CatalogEntityListView } from '@/features/catalog/CatalogEntityListView';
import { useDeleteDrinkMutation, useDrinks } from '@/features/catalog/api/hooks';
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
    [],
  );

  return (
    <CatalogEntityListView
      title="Drink list"
      description="Browse drinks first, then use the separate form surface to create or edit a selected drink."
      sectionTitle="Drinks"
      sectionDescription="Search, sort, and open a drink record to edit."
      data={drinks.data?.items ?? []}
      columns={columns}
      searchPlaceholder="Search drinks"
      newTo="/catalog/drinks/new"
      getRowAriaLabel={(row) => `Open drink ${row.name}`}
      renderMobileCard={(row) => (
        <div className="space-y-4">
          <div className="space-y-1">
            <p className="text-lg font-semibold text-foreground">{row.name}</p>
            <p className="text-sm text-muted-foreground">{row.category || 'Unassigned category'}</p>
          </div>
          <dl className="grid gap-3 text-sm">
            <div className="grid gap-1">
              <dt className="text-xs uppercase tracking-[0.22em] text-muted-foreground">Method</dt>
              <dd className="text-foreground">{row.method || 'Not provided'}</dd>
            </div>
            <div className="grid gap-1">
              <dt className="text-xs uppercase tracking-[0.22em] text-muted-foreground">Garnish</dt>
              <dd className="text-foreground">{row.garnish || 'Not provided'}</dd>
            </div>
            <div className="grid gap-1">
              <dt className="text-xs uppercase tracking-[0.22em] text-muted-foreground">Tags</dt>
              <dd className="text-foreground">
                {joinLines(row.tags.map((tag) => tag.name)) || 'No tags assigned'}
              </dd>
            </div>
          </dl>
          <div className="flex justify-end">
            <Button
              variant="ghost"
              size="sm"
              onClick={(event) => {
                event.stopPropagation();
                setDrinkPendingDelete(row);
              }}
            >
              Delete
            </Button>
          </div>
        </div>
      )}
      onRowClick={(row) =>
        navigate({ to: '/catalog/drinks/$drinkId', params: { drinkId: row.id } })
      }
      errorMessage={deleteDrinkMutation.error?.message ?? null}
      pendingDelete={drinkPendingDelete}
      setPendingDelete={setDrinkPendingDelete}
      confirmDeleteTitle="Delete drink"
      getConfirmDeleteDescription={(row) =>
        `Delete drink "${row.name}"? This action cannot be undone.`
      }
      confirmDeleteLabel="Delete drink"
      onConfirmDelete={(row) =>
        deleteDrinkMutation.mutate(row.id, {
          onSuccess: () => setDrinkPendingDelete(null),
        })
      }
    />
  );
}
