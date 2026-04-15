import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PreferencesPage } from '@/pages/PreferencesPage';
import * as profileHooks from '@/features/profile/hooks';

vi.mock('@/features/profile/hooks');

describe('PreferencesPage', () => {
  it('saves updated profile ingredient preferences', async () => {
    const user = userEvent.setup();
    const saveSpy = vi.fn().mockResolvedValue(undefined);

    vi.mocked(profileHooks.useCustomerIngredients).mockReturnValue({
      data: [
        { id: 'gin', name: 'Gin', notableBrands: ['Plymouth'] },
        { id: 'campari', name: 'Campari', notableBrands: [] },
      ],
    } as unknown as ReturnType<typeof profileHooks.useCustomerIngredients>);
    vi.mocked(profileHooks.useCustomerProfile).mockReturnValue({
      data: {
        favoriteIngredientIds: [],
        dislikedIngredientIds: [],
        prohibitedIngredientIds: [],
        ownedIngredientIds: [],
      },
    } as unknown as ReturnType<typeof profileHooks.useCustomerProfile>);
    vi.mocked(profileHooks.useSaveCustomerProfileMutation).mockReturnValue({
      mutateAsync: saveSpy,
      isPending: false,
    } as unknown as ReturnType<typeof profileHooks.useSaveCustomerProfileMutation>);

    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <PreferencesPage />
      </QueryClientProvider>,
    );

    await user.click(screen.getAllByRole('checkbox', { name: 'Select Gin' })[0]);
    await user.click(screen.getByRole('button', { name: 'Save preferences' }));

    expect(saveSpy).toHaveBeenCalledWith({
      favoriteIngredientIds: ['gin'],
      dislikedIngredientIds: [],
      prohibitedIngredientIds: [],
      ownedIngredientIds: [],
    });
  });
});
