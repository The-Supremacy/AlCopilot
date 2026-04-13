import { useMemo, useState } from 'react';
import { Link, useNavigate } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { IngredientDto } from '@alcopilot/management-api-client';
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
import { useDeleteIngredientMutation, useIngredients } from '@/features/catalog/useCatalogData';
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
          getRowAriaLabel={(row) => `Open ingredient ${row.name}`}
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

      <AlertDialog
        open={ingredientPendingDelete !== null}
        onOpenChange={(open: boolean) => {
          if (!open) {
            setIngredientPendingDelete(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete ingredient</AlertDialogTitle>
            <AlertDialogDescription>
              {ingredientPendingDelete
                ? `Delete ingredient "${ingredientPendingDelete.name}"? This action cannot be undone.`
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
                  if (!ingredientPendingDelete) {
                    return;
                  }

                  deleteIngredientMutation.mutate(ingredientPendingDelete.id, {
                    onSuccess: () => setIngredientPendingDelete(null),
                  });
                }}
              >
                Delete ingredient
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </CatalogShell>
  );
}
