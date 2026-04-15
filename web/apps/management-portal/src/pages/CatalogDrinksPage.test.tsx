import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogDrinksPage } from '@/pages/CatalogDrinksPage';

const navigateSpy = vi.fn();
const deleteDrinkMutation = {
  mutate: vi.fn(),
  error: null as null | Error,
};

vi.mock('@/features/catalog/api/hooks', () => ({
  useDrinks: () => ({
    data: {
      items: [
        {
          id: 'drink-1',
          name: 'Negroni',
          category: 'Classic',
          method: 'Stir',
          garnish: 'Orange peel',
          tags: [{ id: 'tag-1', name: 'Bitter' }],
        },
      ],
    },
  }),
  useDeleteDrinkMutation: () => deleteDrinkMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => navigateSpy,
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogDrinksPage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  navigateSpy.mockReset();
  deleteDrinkMutation.mutate.mockReset();
  deleteDrinkMutation.error = null;
});

test('renders mobile card content and keeps delete action working', async () => {
  const user = userEvent.setup();
  deleteDrinkMutation.mutate.mockImplementation(
    (_id: string, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    },
  );

  renderPage();

  expect(screen.getAllByText('Negroni').length).toBeGreaterThan(0);
  expect(screen.getAllByText('Classic').length).toBeGreaterThan(0);
  expect(screen.getAllByText('Orange peel').length).toBeGreaterThan(0);

  await user.click(screen.getAllByRole('button', { name: 'Delete' })[0]);
  await user.click(screen.getByRole('button', { name: 'Delete drink' }));

  expect(deleteDrinkMutation.mutate).toHaveBeenCalledWith('drink-1', expect.any(Object));
});
