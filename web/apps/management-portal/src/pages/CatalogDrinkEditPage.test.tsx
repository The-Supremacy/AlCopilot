import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { CatalogDrinkEditPage } from '@/pages/CatalogDrinkEditPage';

const navigateSpy = vi.fn();
const updateDrinkMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
};
const deleteDrinkMutation = {
  mutate: vi.fn(),
  error: null as null | Error,
};

let drinkId = 'drink-1';
type DrinkDetails = {
  id: string;
  name: string;
  category: string;
  description: string;
  method: string;
  garnish: string;
  imageUrl: string;
  tags: Array<{ id: string; name: string }>;
  recipeEntries: Array<{
    ingredient: { id: string; name: string };
    quantity: string;
    recommendedBrand: string;
  }>;
};

const drinkQuery = {
  data: {
    id: 'drink-1',
    name: 'Negroni',
    category: 'Contemporary Classics',
    description: 'Classic aperitif.',
    method: 'Stir',
    garnish: 'Orange peel',
    imageUrl: 'https://example.com/negroni.png',
    tags: [{ id: 'tag-1', name: 'Classic' }],
    recipeEntries: [
      {
        ingredient: { id: 'ingredient-1', name: 'Gin' },
        quantity: '1 oz',
        recommendedBrand: 'Tanqueray',
      },
    ],
  } as DrinkDetails | null,
  isLoading: false,
};

vi.mock('@/features/catalog/api/hooks', () => ({
  useDrink: () => drinkQuery,
  useTags: () => ({ data: [{ id: 'tag-1', name: 'Classic' }] }),
  useIngredients: () => ({ data: [{ id: 'ingredient-1', name: 'Gin' }] }),
  useUpdateDrinkMutation: () => updateDrinkMutation,
  useDeleteDrinkMutation: () => deleteDrinkMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useNavigate: () => navigateSpy,
  useParams: () => ({ drinkId }),
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <CatalogDrinkEditPage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  drinkId = 'drink-1';
  navigateSpy.mockReset();
  updateDrinkMutation.mutateAsync.mockReset();
  updateDrinkMutation.mutateAsync.mockResolvedValue(undefined);
  updateDrinkMutation.error = null;
  deleteDrinkMutation.mutate.mockReset();
  deleteDrinkMutation.error = null;
  drinkQuery.isLoading = false;
  drinkQuery.data = {
    id: 'drink-1',
    name: 'Negroni',
    category: 'Contemporary Classics',
    description: 'Classic aperitif.',
    method: 'Stir',
    garnish: 'Orange peel',
    imageUrl: 'https://example.com/negroni.png',
    tags: [{ id: 'tag-1', name: 'Classic' }],
    recipeEntries: [
      {
        ingredient: { id: 'ingredient-1', name: 'Gin' },
        quantity: '1 oz',
        recommendedBrand: 'Tanqueray',
      },
    ],
  };
});

test('hydrates and submits a normalized update payload', async () => {
  const user = userEvent.setup();
  renderPage();

  expect(await screen.findByDisplayValue('Negroni')).toBeInTheDocument();
  expect(screen.getByDisplayValue('Stir')).toBeInTheDocument();

  await user.clear(screen.getByLabelText('Drink category'));
  await user.clear(screen.getByLabelText('Description'));
  await user.clear(screen.getByLabelText('Method'));
  await user.clear(screen.getByLabelText('Garnish'));
  await user.clear(screen.getByLabelText('Image URL'));
  await user.click(screen.getByRole('button', { name: 'Save' }));

  expect(updateDrinkMutation.mutateAsync).toHaveBeenCalledWith({
    id: 'drink-1',
    input: {
      name: 'Negroni',
      category: null,
      description: null,
      method: null,
      garnish: null,
      imageUrl: null,
      tagIds: ['tag-1'],
      recipeEntries: [
        {
          ingredientId: 'ingredient-1',
          quantity: '1 oz',
          recommendedBrand: 'Tanqueray',
        },
      ],
    },
  });
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/drinks' });
});

test('confirms and deletes the selected drink', async () => {
  const user = userEvent.setup();
  deleteDrinkMutation.mutate.mockImplementation(
    (_id: string, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    },
  );

  renderPage();

  await user.click(await screen.findByRole('button', { name: 'Delete' }));
  expect(screen.getByRole('heading', { name: 'Delete drink' })).toBeInTheDocument();
  expect(
    screen.getByText('Delete drink "Negroni"? This action cannot be undone.'),
  ).toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Delete drink' }));

  expect(deleteDrinkMutation.mutate).toHaveBeenCalledWith('drink-1', expect.any(Object));
  expect(navigateSpy).toHaveBeenCalledWith({ to: '/catalog/drinks' });
});

test('shows the loading state while drink details are loading', () => {
  drinkQuery.isLoading = true;
  drinkQuery.data = null;

  renderPage();

  expect(screen.getByText('Loading...')).toBeInTheDocument();
});

test('shows a not-found state when the drink does not exist', () => {
  drinkQuery.data = null;

  renderPage();

  expect(screen.getByText('Drink not found.')).toBeInTheDocument();
});
