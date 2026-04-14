import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogIngredientEditPage } from '@/pages/CatalogIngredientEditPage';

const navigateSpy = vi.fn();
const updateIngredientMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
};
const deleteIngredientMutation = {
  mutate: vi.fn(),
  error: null as null | Error,
};

const ingredientsQuery = {
  data: [{ id: 'ingredient-1', name: 'Gin', notableBrands: ['Tanqueray'] }],
  isLoading: false,
};

let ingredientId = 'ingredient-1';

vi.mock('@/features/catalog/api/hooks', () => ({
  useIngredients: () => ingredientsQuery,
  useUpdateIngredientMutation: () => updateIngredientMutation,
  useDeleteIngredientMutation: () => deleteIngredientMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => navigateSpy,
  useParams: () => ({ ingredientId }),
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogIngredientEditPage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  ingredientId = 'ingredient-1';
  navigateSpy.mockReset();
  updateIngredientMutation.mutateAsync.mockReset();
  updateIngredientMutation.mutateAsync.mockResolvedValue(undefined);
  updateIngredientMutation.error = null;
  deleteIngredientMutation.mutate.mockReset();
  deleteIngredientMutation.error = null;
});

test('hydrates and submits parsed notable brands for the selected ingredient', async () => {
  const user = userEvent.setup();
  renderPage();

  expect(await screen.findByDisplayValue('Gin')).toBeInTheDocument();
  expect(screen.getByDisplayValue('Tanqueray')).toBeInTheDocument();

  await user.clear(screen.getByLabelText('Ingredient name'));
  await user.type(screen.getByLabelText('Ingredient name'), 'London Dry Gin');
  await user.clear(screen.getByLabelText('Notable brands'));
  await user.type(screen.getByLabelText('Notable brands'), 'Tanqueray, Beefeater');
  await user.click(screen.getByRole('button', { name: 'Save' }));

  expect(updateIngredientMutation.mutateAsync).toHaveBeenCalledWith({
    id: 'ingredient-1',
    input: {
      name: 'London Dry Gin',
      notableBrands: ['Tanqueray', 'Beefeater'],
    },
  });
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/ingredients' });
});

test('confirms and deletes the selected ingredient', async () => {
  const user = userEvent.setup();
  deleteIngredientMutation.mutate.mockImplementation(
    (_id: string, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    },
  );

  renderPage();

  await user.click(await screen.findByRole('button', { name: 'Delete' }));
  expect(screen.getByRole('heading', { name: 'Delete ingredient' })).toBeInTheDocument();
  expect(
    screen.getByText('Delete ingredient "Gin"? This action cannot be undone.'),
  ).toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Delete ingredient' }));

  expect(deleteIngredientMutation.mutate).toHaveBeenCalledWith('ingredient-1', expect.any(Object));
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/ingredients' });
});

test('shows a not-found state when the ingredient is missing', () => {
  ingredientId = 'missing-ingredient';

  renderPage();

  expect(screen.getByText('Ingredient not found.')).toBeInTheDocument();
});
