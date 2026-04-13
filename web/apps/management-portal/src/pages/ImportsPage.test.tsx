import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { ImportsPage } from '@/pages/ImportsPage';

const startImportMutation = {
  mutateAsync: vi.fn().mockResolvedValue({ id: 'batch-1' }),
  error: null,
  isPending: false,
};

const noopMutation = {
  mutate: vi.fn(),
  mutateAsync: vi.fn(),
  error: null,
  isPending: false,
};

vi.mock('@/features/imports/useImportData', () => ({
  useImportHistory: () => ({ data: [] }),
  useImportBatch: () => ({ data: null }),
  useStartImportMutation: () => startImportMutation,
  useApplyImportBatchMutation: () => noopMutation,
  useCancelImportBatchMutation: () => noopMutation,
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <ImportsPage />
    </QueryClientProvider>,
  );
}

test('starts an import from the workspace form', async () => {
  const user = userEvent.setup();
  renderPage();

  await user.click(screen.getByRole('button', { name: 'Import default preset' }));

  expect(startImportMutation.mutateAsync).toHaveBeenCalled();
});
