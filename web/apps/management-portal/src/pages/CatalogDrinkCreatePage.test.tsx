import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogDrinkCreatePage } from '@/pages/CatalogDrinkCreatePage';

const navigateSpy = vi.fn();
const createDrinkMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
};

vi.mock('@/features/catalog/useCatalogData', () => ({
  useCreateDrinkMutation: () => createDrinkMutation,
  useTags: () => ({ data: [{ id: 'tag-1', name: 'Classic' }] }),
  useIngredients: () => ({ data: [{ id: 'ingredient-1', name: 'Gin' }] }),
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => navigateSpy,
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogDrinkCreatePage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  navigateSpy.mockReset();
  createDrinkMutation.mutateAsync.mockReset();
  createDrinkMutation.mutateAsync.mockResolvedValue(undefined);
  createDrinkMutation.error = null;
});

test('submits a normalized drink payload and filters incomplete recipe entries', async () => {
  const user = userEvent.setup();
  renderPage();

  await user.type(screen.getByLabelText('Drink name'), 'Negroni');
  await user.type(screen.getByLabelText('Drink category'), 'Contemporary Classics');
  await user.type(screen.getByLabelText('Image URL'), 'https://example.com/negroni.png');
  await user.type(screen.getByLabelText('Description'), 'Bittersweet classic.');
  await user.type(screen.getByLabelText('Method'), 'Stir with ice.');
  await user.type(screen.getByLabelText('Garnish'), 'Orange peel');
  await user.click(screen.getByLabelText('Classic'));
  await user.selectOptions(screen.getByLabelText('Recipe ingredient 1'), 'ingredient-1');
  await user.type(screen.getByLabelText('Recipe quantity 1'), '1 oz');
  await user.type(screen.getByLabelText('Recommended brand 1'), 'Tanqueray');
  await user.click(screen.getByRole('button', { name: 'Add ingredient' }));
  await user.type(screen.getByLabelText('Recipe quantity 2'), '2 oz');
  await user.click(screen.getByRole('button', { name: 'Save' }));

  expect(createDrinkMutation.mutateAsync).toHaveBeenCalledWith({
    name: 'Negroni',
    category: 'Contemporary Classics',
    description: 'Bittersweet classic.',
    method: 'Stir with ice.',
    garnish: 'Orange peel',
    imageUrl: 'https://example.com/negroni.png',
    tagIds: ['tag-1'],
    recipeEntries: [
      {
        ingredientId: 'ingredient-1',
        quantity: '1 oz',
        recommendedBrand: 'Tanqueray',
      },
    ],
  });
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/drinks' });
});
