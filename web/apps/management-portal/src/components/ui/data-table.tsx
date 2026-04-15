import type { KeyboardEvent, ReactNode } from 'react';
import { useState } from 'react';
import { ArrowUpDown, ChevronDown } from 'lucide-react';
import type {
  Column,
  ColumnDef,
  Header,
  Row,
  SortingState,
  Table as TanStackTable,
  VisibilityState,
} from '@tanstack/react-table';
import {
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getSortedRowModel,
  useReactTable,
} from '@tanstack/react-table';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { cn } from '@/lib/utils';

type DataTableProps<TData> = {
  columns: ColumnDef<TData, unknown>[];
  data: TData[];
  searchPlaceholder?: string;
  toolbarAction?: ReactNode;
  onRowClick?: (row: TData) => void;
  getRowAriaLabel?: (row: TData) => string;
  enableColumnToggle?: boolean;
  renderMobileCard?: (row: TData) => ReactNode;
};

export function DataTable<TData>({
  columns,
  data,
  searchPlaceholder = 'Search',
  toolbarAction,
  onRowClick,
  getRowAriaLabel,
  enableColumnToggle = true,
  renderMobileCard,
}: DataTableProps<TData>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});

  const table = useReactTable({
    data,
    columns,
    state: {
      sorting,
      globalFilter,
      columnVisibility,
    },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    onColumnVisibilityChange: setColumnVisibility,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
  });

  const hasRows = table.getRowModel().rows.length > 0;
  const rows = table.getRowModel().rows;
  const sortableColumns = table.getAllLeafColumns().filter((column) => column.getCanSort());

  return (
    <div className="space-y-4">
      <DataTableToolbar
        table={table}
        sorting={sorting}
        setSorting={setSorting}
        searchPlaceholder={searchPlaceholder}
        globalFilter={globalFilter}
        setGlobalFilter={setGlobalFilter}
        toolbarAction={toolbarAction}
        enableColumnToggle={enableColumnToggle}
        showMobileSort={Boolean(renderMobileCard)}
      />

      <DataTableMobileCards
        rows={rows}
        hasRows={hasRows}
        onRowClick={onRowClick}
        getRowAriaLabel={getRowAriaLabel}
        renderMobileCard={renderMobileCard}
      />

      <DataTableDesktopTable
        table={table}
        rows={rows}
        columns={columns}
        hasRows={hasRows}
        onRowClick={onRowClick}
        getRowAriaLabel={getRowAriaLabel}
        hideOnMobile={Boolean(renderMobileCard)}
      />
    </div>
  );
}

type DataTableToolbarProps<TData> = {
  table: TanStackTable<TData>;
  sorting: SortingState;
  setSorting: (updater: SortingState) => void;
  searchPlaceholder: string;
  globalFilter: string;
  setGlobalFilter: (value: string) => void;
  toolbarAction?: ReactNode;
  enableColumnToggle: boolean;
  showMobileSort: boolean;
};

function DataTableToolbar<TData>({
  table,
  sorting,
  setSorting,
  searchPlaceholder,
  globalFilter,
  setGlobalFilter,
  toolbarAction,
  enableColumnToggle,
  showMobileSort,
}: DataTableToolbarProps<TData>) {
  const sortableColumns = table.getAllLeafColumns().filter((column) => column.getCanSort());

  return (
    <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div className="flex flex-col gap-3 md:flex-row md:items-center">
        <Input
          value={globalFilter}
          onChange={(event) => setGlobalFilter(event.target.value)}
          placeholder={searchPlaceholder}
          className="md:max-w-xs"
        />
        {showMobileSort && sortableColumns.length > 0 ? (
          <label className="flex items-center gap-2 text-sm text-muted-foreground md:hidden">
            <span className="shrink-0">Sort by</span>
            <select
              value={sorting[0]?.id ?? ''}
              onChange={(event) => {
                const nextColumnId = event.target.value;
                setSorting(nextColumnId ? [{ id: nextColumnId, desc: false }] : []);
              }}
              className="flex h-10 min-w-0 flex-1 rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              aria-label="Sort rows"
            >
              <option value="">Default order</option>
              {sortableColumns.map((column) => (
                <option key={column.id} value={column.id}>
                  {getColumnLabel(column)}
                </option>
              ))}
            </select>
          </label>
        ) : null}
      </div>
      <div className="flex flex-wrap gap-2">
        {toolbarAction}
        {enableColumnToggle ? <DataTableColumnToggle table={table} /> : null}
      </div>
    </div>
  );
}

function DataTableColumnToggle<TData>({ table }: { table: TanStackTable<TData> }) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="outline" size="sm" className="hidden gap-2 md:inline-flex">
          Columns
          <ChevronDown className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        {table
          .getAllColumns()
          .filter((column) => column.getCanHide())
          .map((column) => (
            <DropdownMenuCheckboxItem
              key={column.id}
              checked={column.getIsVisible()}
              onCheckedChange={(value) => column.toggleVisibility(Boolean(value))}
              className="gap-3 pr-3"
            >
              <span className="flex-1">{getColumnLabel(column)}</span>
              <span className="text-xs text-muted-foreground">
                {column.getIsVisible() ? 'Shown' : 'Hidden'}
              </span>
            </DropdownMenuCheckboxItem>
          ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

type DataTableMobileCardsProps<TData> = {
  rows: Row<TData>[];
  hasRows: boolean;
  onRowClick?: (row: TData) => void;
  getRowAriaLabel?: (row: TData) => string;
  renderMobileCard?: (row: TData) => ReactNode;
};

function DataTableMobileCards<TData>({
  rows,
  hasRows,
  onRowClick,
  getRowAriaLabel,
  renderMobileCard,
}: DataTableMobileCardsProps<TData>) {
  if (!renderMobileCard) {
    return null;
  }

  return (
    <div className="space-y-3 md:hidden">
      {hasRows ? (
        rows.map((row) => (
          <div
            key={row.id}
            className={cn(
              'rounded-2xl border border-border bg-background/85 p-4 shadow-soft',
              onRowClick ? 'cursor-pointer' : undefined,
            )}
            {...getRowInteractionProps({
              row: row.original,
              onRowClick,
              getRowAriaLabel,
            })}
          >
            {renderMobileCard(row.original)}
          </div>
        ))
      ) : (
        <EmptyStateCard />
      )}
    </div>
  );
}

type DataTableDesktopTableProps<TData> = {
  table: TanStackTable<TData>;
  rows: Row<TData>[];
  columns: ColumnDef<TData, unknown>[];
  hasRows: boolean;
  onRowClick?: (row: TData) => void;
  getRowAriaLabel?: (row: TData) => string;
  hideOnMobile: boolean;
};

function DataTableDesktopTable<TData>({
  table,
  rows,
  columns,
  hasRows,
  onRowClick,
  getRowAriaLabel,
  hideOnMobile,
}: DataTableDesktopTableProps<TData>) {
  return (
    <div
      className={cn(
        'rounded-2xl border border-border bg-background/80',
        hideOnMobile ? 'hidden md:block' : undefined,
      )}
    >
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id}>
                  {header.isPlaceholder ? null : <DataTableHeaderCell header={header} />}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {hasRows ? (
            rows.map((row) => (
              <TableRow
                key={row.id}
                className={cn(onRowClick ? 'cursor-pointer' : undefined)}
                {...getRowInteractionProps({
                  row: row.original,
                  onRowClick,
                  getRowAriaLabel,
                })}
              >
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </TableCell>
                ))}
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell
                colSpan={columns.length}
                className="py-10 text-center text-sm text-muted-foreground"
              >
                No results found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}

function DataTableHeaderCell<TData>({ header }: { header: Header<TData, unknown> }) {
  if (header.column.getCanSort()) {
    return (
      <Button
        variant="ghost"
        className="h-auto gap-2 px-0 text-xs font-semibold uppercase tracking-wide text-muted-foreground"
        onClick={header.column.getToggleSortingHandler()}
      >
        {flexRender(header.column.columnDef.header, header.getContext())}
        <ArrowUpDown className="h-3 w-3" />
      </Button>
    );
  }

  return (
    <span className="inline-flex h-auto px-0 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
      {flexRender(header.column.columnDef.header, header.getContext())}
    </span>
  );
}

function EmptyStateCard() {
  return (
    <div className="rounded-2xl border border-border bg-background/85 px-4 py-10 text-center text-sm text-muted-foreground">
      No results found.
    </div>
  );
}

function getColumnLabel<TData>(column: Column<TData, unknown>) {
  const metaLabel = (column.columnDef.meta as { label?: string } | undefined)?.label;
  if (metaLabel) return metaLabel;

  if (typeof column.columnDef.header === 'string') return column.columnDef.header;

  return column.id
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[-_]/g, ' ')
    .replace(/\b\w/g, (value) => value.toUpperCase());
}

function getRowInteractionProps<TData>({
  row,
  onRowClick,
  getRowAriaLabel,
}: {
  row: TData;
  onRowClick?: (row: TData) => void;
  getRowAriaLabel?: (row: TData) => string;
}) {
  if (!onRowClick) {
    return {};
  }

  return {
    onClick: () => onRowClick(row),
    onKeyDown: (event: KeyboardEvent<HTMLElement>) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        onRowClick(row);
      }
    },
    tabIndex: 0,
    role: 'link' as const,
    'aria-label': getRowAriaLabel?.(row),
  };
}
