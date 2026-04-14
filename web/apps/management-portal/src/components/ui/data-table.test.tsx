import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ColumnDef } from '@tanstack/react-table';
import { vi } from 'vitest';
import { DataTable } from '@/components/ui/data-table';

type Row = {
  id: string;
  name: string;
  category: string;
};

const rows: Row[] = [
  { id: '1', name: 'Negroni', category: 'Classic' },
  { id: '2', name: 'Penicillin', category: 'Modern' },
];

const columns: ColumnDef<Row>[] = [
  {
    accessorKey: 'name',
    header: 'Name',
    meta: { label: 'Name' },
  },
  {
    accessorKey: 'category',
    header: 'Category',
    meta: { label: 'Category' },
    enableSorting: false,
  },
];

test('renders sortable headers as buttons and non-sortable headers as static text', () => {
  render(<DataTable columns={columns} data={rows} />);

  expect(screen.getByRole('button', { name: /name/i })).toBeInTheDocument();
  expect(screen.queryByRole('button', { name: /category/i })).not.toBeInTheDocument();
  expect(screen.getByText('Category')).toBeInTheDocument();
});

test('renders mobile cards from the same row model and applies filtering', async () => {
  const user = userEvent.setup();

  render(
    <DataTable
      columns={columns}
      data={rows}
      searchPlaceholder="Search drinks"
      renderMobileCard={(row) => (
        <div>
          <p>{row.name}</p>
          <p>{row.category}</p>
        </div>
      )}
    />,
  );

  expect(screen.getAllByText('Negroni').length).toBeGreaterThan(0);
  expect(screen.getAllByText('Penicillin').length).toBeGreaterThan(0);

  await user.type(screen.getByPlaceholderText('Search drinks'), 'Negroni');

  expect(screen.getAllByText('Negroni').length).toBeGreaterThan(0);
  expect(screen.queryByText('Penicillin')).not.toBeInTheDocument();
});

test('supports keyboard row activation for mobile cards', async () => {
  const user = userEvent.setup();
  const onRowClick = vi.fn();

  render(
    <DataTable
      columns={columns}
      data={rows}
      onRowClick={onRowClick}
      getRowAriaLabel={(row) => `Open ${row.name}`}
      renderMobileCard={(row) => <p>{row.name}</p>}
    />,
  );

  screen.getAllByRole('link', { name: 'Open Negroni' })[0].focus();
  await user.keyboard('{Enter}');

  expect(onRowClick).toHaveBeenCalledWith(rows[0]);
});
