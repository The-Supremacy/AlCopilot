import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogTagCreatePage } from '@/pages/CatalogTagCreatePage';

const createTagMutation = { mutateAsync: vi.fn(), mutate: vi.fn(), error: null };

vi.mock('@/features/catalog/useCatalogData', () => ({
  useCreateTagMutation: () => createTagMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => () => {},
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogTagCreatePage />
    </QueryClientProvider>,
  );
}

test('creates a tag from the tag create page', async () => {
  const user = userEvent.setup();
  renderPage();

  await user.type(screen.getByPlaceholderText('Classic'), 'Bitter');
  await user.click(screen.getByRole('button', { name: 'Save' }));

  expect(createTagMutation.mutateAsync).toHaveBeenCalledWith({ name: 'Bitter' });
});
