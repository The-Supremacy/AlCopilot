import type { ReactNode } from 'react';
import { useState } from 'react';
import { ArrowUpDown, ChevronDown } from 'lucide-react';
import type { ColumnDef, SortingState, VisibilityState } from '@tanstack/react-table';
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
  enableColumnToggle?: boolean;
};

export function DataTable<TData>({
  columns,
  data,
  searchPlaceholder = 'Search',
  toolbarAction,
  onRowClick,
  enableColumnToggle = true,
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

  function getColumnToggleLabel(column: ReturnType<typeof table.getAllColumns>[number]) {
    const metaLabel = (column.columnDef.meta as { label?: string } | undefined)?.label;
    if (metaLabel) return metaLabel;

    if (typeof column.columnDef.header === 'string') return column.columnDef.header;

    return column.id
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/[-_]/g, ' ')
      .replace(/\b\w/g, (value) => value.toUpperCase());
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <Input
          value={globalFilter ?? ''}
          onChange={(event) => setGlobalFilter(event.target.value)}
          placeholder={searchPlaceholder}
          className="md:max-w-xs"
        />
        <div className="flex flex-wrap gap-2">
          {toolbarAction}
          {enableColumnToggle ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" size="sm" className="gap-2">
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
                      <span className="flex-1">{getColumnToggleLabel(column)}</span>
                      <span className="text-xs text-muted-foreground">
                        {column.getIsVisible() ? 'Shown' : 'Hidden'}
                      </span>
                    </DropdownMenuCheckboxItem>
                  ))}
              </DropdownMenuContent>
            </DropdownMenu>
          ) : null}
        </div>
      </div>

      <div className="rounded-2xl border border-border bg-background/80">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder ? null : (
                      <Button
                        variant="ghost"
                        className="h-auto gap-2 px-0 text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {header.column.getCanSort() ? <ArrowUpDown className="h-3 w-3" /> : null}
                      </Button>
                    )}
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
                  onClick={onRowClick ? () => onRowClick(row.original) : undefined}
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
    </div>
  );
}
