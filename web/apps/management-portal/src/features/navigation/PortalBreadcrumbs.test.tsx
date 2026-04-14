import type { AnchorHTMLAttributes } from 'react';
import { render, screen } from '@testing-library/react';
import { vi } from 'vitest';
import { PortalBreadcrumbs } from './PortalBreadcrumbs';

const mockedUseMatches = vi.fn();

vi.mock('@tanstack/react-router', async () => {
  const actual =
    await vi.importActual<typeof import('@tanstack/react-router')>('@tanstack/react-router');

  return {
    ...actual,
    useMatches: (options?: { select?: (matches: unknown[]) => unknown }) => {
      const matches = mockedUseMatches();
      return options?.select ? options.select(matches) : matches;
    },
    Link: ({
      children,
      to,
      ...props
    }: AnchorHTMLAttributes<HTMLAnchorElement> & { to?: string }) => (
      <a href={to} {...props}>
        {children}
      </a>
    ),
  };
});

beforeEach(() => {
  mockedUseMatches.mockReset();
});

test('does not render breadcrumbs when fewer than two items are provided', () => {
  mockedUseMatches.mockReturnValue([
    {
      staticData: {
        breadcrumbs: [{ label: 'Catalog', to: { to: '/catalog' } }],
      },
    },
  ]);

  render(<PortalBreadcrumbs />);

  expect(screen.queryByRole('navigation', { name: 'Breadcrumb' })).not.toBeInTheDocument();
});

test('renders linked ancestors and a current page crumb', () => {
  mockedUseMatches.mockReturnValue([
    {
      staticData: {
        breadcrumbs: [
          { label: 'Catalog', to: { to: '/catalog' } },
          { label: 'Drinks', to: { to: '/catalog/drinks' } },
          { label: 'Edit drink' },
        ],
      },
    },
  ]);

  render(<PortalBreadcrumbs />);

  expect(screen.getByRole('link', { name: 'Catalog' })).toBeInTheDocument();
  expect(screen.getByRole('link', { name: 'Drinks' })).toBeInTheDocument();
  expect(screen.getByText('Edit drink')).toHaveAttribute('aria-current', 'page');
});
