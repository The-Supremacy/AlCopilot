import { useMemo } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { IngredientDto } from '@alcopilot/management-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { joinLines } from '@/lib/format';
import { useDeleteIngredientMutation, useIngredients } from '@/lib/usePortalData';

export function CatalogIngredientsPage() {
  const navigate = useNavigate();
  const ingredients = useIngredients();
  const deleteIngredientMutation = useDeleteIngredientMutation();

  const columns = useMemo<ColumnDef<IngredientDto>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => <span className="font-medium text-slate-950">{row.original.name}</span>,
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
              if (confirm(`Delete ingredient "${row.original.name}"?`)) {
                deleteIngredientMutation.mutate(row.original.id);
              }
            }}
          >
            Delete
          </Button>
        ),
      },
    ],
    [deleteIngredientMutation],
  );

  return (
    <CatalogShell
      title="Ingredient list"
      description="Browse ingredients separately from drinks, then open a form page to edit brands or add new entries."
    >
      {deleteIngredientMutation.error ? (
        <InlineMessage tone="danger" message={deleteIngredientMutation.error.message} />
      ) : null}

      <SectionCard
        title="Ingredients"
        description="Search, sort, and open an ingredient record to edit."
      >
        <DataTable
          columns={columns}
          data={ingredients.data ?? []}
          searchPlaceholder="Search ingredients"
          toolbarAction={
            <Button asChild>
              <Link to="/catalog/ingredients/new">New</Link>
            </Button>
          }
          onRowClick={(row) =>
            navigate({ to: '/catalog/ingredients/$ingredientId', params: { ingredientId: row.id } })
          }
        />
      </SectionCard>
    </CatalogShell>
  );
}
