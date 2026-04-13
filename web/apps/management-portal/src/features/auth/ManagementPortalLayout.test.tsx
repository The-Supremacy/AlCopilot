import type { AnchorHTMLAttributes } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { ManagementPortalLayout } from '@/features/auth/ManagementPortalLayout';

const mockedUseManagementSession = vi.fn();
const mockedUseLogoutManagementMutation = vi.fn();

vi.mock('@/lib/useManagementAuth', () => ({
  useManagementSession: () => mockedUseManagementSession(),
  useLogoutManagementMutation: () => mockedUseLogoutManagementMutation(),
}));

vi.mock('@tanstack/react-router', async () => {
  const actual =
    await vi.importActual<typeof import('@tanstack/react-router')>('@tanstack/react-router');

  return {
    ...actual,
    Link: ({ children, ...props }: AnchorHTMLAttributes<HTMLAnchorElement>) => (
      <a {...props}>{children}</a>
    ),
  };
});

function renderLayout() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <ManagementPortalLayout>
        <div>Protected portal content</div>
      </ManagementPortalLayout>
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  mockedUseLogoutManagementMutation.mockReturnValue({
    mutateAsync: vi.fn().mockResolvedValue(undefined),
    isPending: false,
  });
});

test('shows a local sign-in required state for unauthenticated users', () => {
  mockedUseManagementSession.mockReturnValue({
    isLoading: false,
    isError: false,
    data: {
      isAuthenticated: false,
      displayName: null,
      roles: [],
      isAdmin: false,
      canAccessManagementPortal: false,
    },
  });

  renderLayout();

  expect(screen.getByRole('heading', { name: 'Sign in required' })).toBeInTheDocument();
  expect(screen.queryByText('Protected portal content')).not.toBeInTheDocument();
});

test('shows an access denied state for authenticated non-manager users', () => {
  mockedUseManagementSession.mockReturnValue({
    isLoading: false,
    isError: false,
    data: {
      isAuthenticated: true,
      displayName: 'user@alcopilot.local',
      roles: ['user'],
      isAdmin: false,
      canAccessManagementPortal: false,
    },
  });

  renderLayout();

  expect(screen.getByRole('heading', { name: 'Management access denied' })).toBeInTheDocument();
  expect(screen.queryByText('Protected portal content')).not.toBeInTheDocument();
});

test('renders the authenticated shell for manager sessions', async () => {
  const logout = vi.fn().mockResolvedValue(undefined);

  mockedUseManagementSession.mockReturnValue({
    isLoading: false,
    isError: false,
    data: {
      isAuthenticated: true,
      displayName: 'manager@alcopilot.local',
      roles: ['manager', 'user'],
      isAdmin: false,
      canAccessManagementPortal: true,
    },
  });
  mockedUseLogoutManagementMutation.mockReturnValue({
    mutateAsync: logout,
    isPending: false,
  });

  renderLayout();

  expect(screen.getByText('Protected portal content')).toBeInTheDocument();
  expect(screen.getByText('manager@alcopilot.local')).toBeInTheDocument();

  await userEvent.setup().click(screen.getByRole('button', { name: 'Sign out' }));

  expect(logout).toHaveBeenCalled();
});
