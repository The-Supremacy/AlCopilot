import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { CustomerPortalLayout } from '@/features/auth/CustomerPortalLayout';
import * as authHooks from '@/state/auth/useCustomerAuth';
import * as recommendationHooks from '@/features/recommendations/hooks';

vi.mock('@/state/auth/useCustomerAuth');
vi.mock('@/features/recommendations/hooks');

describe('CustomerPortalLayout', () => {
  it('renders a sign-in-required state for anonymous customers', () => {
    vi.mocked(authHooks.useCustomerSession).mockReturnValue({
      isLoading: false,
      isError: false,
      data: {
        isAuthenticated: false,
        displayName: null,
        roles: [],
        canAccessCustomerPortal: false,
      },
    } as unknown as ReturnType<typeof authHooks.useCustomerSession>);
    vi.mocked(authHooks.useLogoutCustomerMutation).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof authHooks.useLogoutCustomerMutation>);
    vi.mocked(recommendationHooks.useRecommendationSessions).mockReturnValue({
      data: [],
    } as unknown as ReturnType<typeof recommendationHooks.useRecommendationSessions>);

    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <CustomerPortalLayout>
          <div>Protected content</div>
        </CustomerPortalLayout>
      </QueryClientProvider>,
    );

    expect(screen.getByText('Sign in required')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Continue to sign in' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Register first' })).toBeInTheDocument();
  });
});
