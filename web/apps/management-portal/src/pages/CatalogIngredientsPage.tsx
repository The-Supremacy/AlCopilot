import { useMemo, useState } from 'react';
import { useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { IngredientDto } from '@alcopilot/management-api-client';
import { Button } from '@/components/ui/button';
import { CatalogEntityListView } from '@/features/catalog/CatalogEntityListView';
import { useDeleteIngredientMutation, useIngredients } from '@/features/catalog/api/hooks';
import { joinLines } from '@/lib/format';

export function CatalogIngredientsPage() {
  const navigate = useNavigate();
  const ingredients = useIngredients();
  const deleteIngredientMutation = useDeleteIngredientMutation();
  const [ingredientPendingDelete, setIngredientPendingDelete] = useState<IngredientDto | null>(
    null,
  );

  const columns = useMemo<ColumnDef<IngredientDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-foreground">{row.original.name}</span>,
      },
      {
        id: 'brands',
        header: 'Notable brands',
        cell: ({ row }) => joinLines(row.original.notableBrands) || '—',
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
              setIngredientPendingDelete(row.original);
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
      title="Ingredient list"
      description="Browse ingredients separately from drinks, then open a form page to edit brands or add new entries."
      sectionTitle="Ingredients"
      sectionDescription="Search, sort, and open an ingredient record to edit."
      data={ingredients.data ?? []}
      columns={columns}
      searchPlaceholder="Search ingredients"
      newTo="/catalog/ingredients/new"
      getRowAriaLabel={(row) => `Open ingredient ${row.name}`}
      renderMobileCard={(row) => (
        <div className="space-y-4">
          <div className="space-y-1">
            <p className="text-lg font-semibold text-foreground">{row.name}</p>
            <p className="text-sm text-muted-foreground">
              {row.notableBrands.length} notable brand
              {row.notableBrands.length === 1 ? '' : 's'}
            </p>
          </div>
          <div className="grid gap-1 text-sm">
            <p className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
              Notable brands
            </p>
            <p className="text-foreground">
              {joinLines(row.notableBrands) || 'No notable brands recorded'}
            </p>
          </div>
          <div className="flex justify-end">
            <Button
              variant="ghost"
              size="sm"
              onClick={(event) => {
                event.stopPropagation();
                setIngredientPendingDelete(row);
              }}
            >
              Delete
            </Button>
          </div>
        </div>
      )}
      onRowClick={(row) =>
        navigate({ to: '/catalog/ingredients/$ingredientId', params: { ingredientId: row.id } })
      }
      errorMessage={deleteIngredientMutation.error?.message ?? null}
      pendingDelete={ingredientPendingDelete}
      setPendingDelete={setIngredientPendingDelete}
      confirmDeleteTitle="Delete ingredient"
      getConfirmDeleteDescription={(row) =>
        `Delete ingredient "${row.name}"? This action cannot be undone.`
      }
      confirmDeleteLabel="Delete ingredient"
      onConfirmDelete={(row) =>
        deleteIngredientMutation.mutate(row.id, {
          onSuccess: () => setIngredientPendingDelete(null),
        })
      }
    />
  );
}
