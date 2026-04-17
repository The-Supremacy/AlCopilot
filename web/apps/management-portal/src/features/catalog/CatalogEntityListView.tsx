import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
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

type CatalogEntityListViewProps<TData> = {
  title: string;
  description: string;
  sectionTitle: string;
  sectionDescription: string;
  data: TData[];
  columns: ColumnDef<TData, unknown>[];
  searchPlaceholder: string;
  newTo: string;
  getRowAriaLabel: (row: TData) => string;
  renderMobileCard: (row: TData) => ReactNode;
  onRowClick: (row: TData) => void;
  errorMessage?: string | null;
  pendingDelete: TData | null;
  setPendingDelete: (value: TData | null) => void;
  confirmDeleteTitle: string;
  getConfirmDeleteDescription: (row: TData) => string;
  confirmDeleteLabel: string;
  onConfirmDelete: (row: TData) => void;
};

export function CatalogEntityListView<TData>({
  title,
  description,
  sectionTitle,
  sectionDescription,
  data,
  columns,
  searchPlaceholder,
  newTo,
  getRowAriaLabel,
  renderMobileCard,
  onRowClick,
  errorMessage,
  pendingDelete,
  setPendingDelete,
  confirmDeleteTitle,
  getConfirmDeleteDescription,
  confirmDeleteLabel,
  onConfirmDelete,
}: CatalogEntityListViewProps<TData>) {
  return (
    <CatalogShell title={title} description={description}>
      {errorMessage ? <InlineMessage tone="danger" message={errorMessage} /> : null}

      <SectionCard title={sectionTitle} description={sectionDescription}>
        <DataTable
          columns={columns}
          data={data}
          searchPlaceholder={searchPlaceholder}
          getRowAriaLabel={getRowAriaLabel}
          renderMobileCard={renderMobileCard}
          toolbarAction={
            <Button asChild>
              <Link to={newTo}>New</Link>
            </Button>
          }
          onRowClick={onRowClick}
        />
      </SectionCard>

      <AlertDialog
        open={pendingDelete !== null}
        onOpenChange={(open: boolean) => {
          if (!open) {
            setPendingDelete(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{confirmDeleteTitle}</AlertDialogTitle>
            <AlertDialogDescription>
              {pendingDelete ? getConfirmDeleteDescription(pendingDelete) : ''}
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
                  if (!pendingDelete) {
                    return;
                  }

                  onConfirmDelete(pendingDelete);
                }}
              >
                {confirmDeleteLabel}
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </CatalogShell>
  );
}
