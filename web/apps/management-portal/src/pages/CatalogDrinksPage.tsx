import { useMemo } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { DrinkDto } from '@alcopilot/management-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { joinLines } from '@/lib/format';
import { useDeleteDrinkMutation, useDrinks } from '@/lib/usePortalData';

export function CatalogDrinksPage() {
  const navigate = useNavigate();
  const drinks = useDrinks();
  const deleteDrinkMutation = useDeleteDrinkMutation();

  const columns = useMemo<ColumnDef<DrinkDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-slate-950">{row.original.name}</span>,
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
              if (confirm(`Delete drink "${row.original.name}"?`)) {
                deleteDrinkMutation.mutate(row.original.id);
              }
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
    </CatalogShell>
  );
}
