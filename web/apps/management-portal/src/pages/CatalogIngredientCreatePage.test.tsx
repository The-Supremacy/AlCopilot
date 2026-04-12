import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogIngredientCreatePage } from '@/pages/CatalogIngredientCreatePage';

const navigateSpy = vi.fn();
const createIngredientMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
};

vi.mock('@/lib/usePortalData', () => ({
  useCreateIngredientMutation: () => createIngredientMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => navigateSpy,
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogIngredientCreatePage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  navigateSpy.mockReset();
  createIngredientMutation.mutateAsync.mockReset();
  createIngredientMutation.mutateAsync.mockResolvedValue(undefined);
  createIngredientMutation.error = null;
});

test('submits parsed notable brands and navigates back to the ingredient list', async () => {
  const user = userEvent.setup();
  renderPage();

  await user.type(screen.getByLabelText('Ingredient name'), 'Gin');
  await user.type(screen.getByLabelText('Notable brands'), 'Tanqueray, Beefeater ,  Plymouth  ');
  await user.click(screen.getByRole('button', { name: 'Save' }));

  expect(createIngredientMutation.mutateAsync).toHaveBeenCalledWith({
    name: 'Gin',
    notableBrands: ['Tanqueray', 'Beefeater', 'Plymouth'],
  });
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/ingredients' });
});

test('renders the create mutation error message', () => {
  createIngredientMutation.error = new Error('Ingredient already exists.');

  renderPage();

  expect(screen.getByText('Ingredient already exists.')).toBeInTheDocument();
});
