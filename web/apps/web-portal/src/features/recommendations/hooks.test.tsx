import {
  submitRecommendationRequest,
  submitRecommendationTurnFeedback,
} from '@alcopilot/customer-api-client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { portalKeys } from '@/state/queryKeys';
import {
  useSubmitRecommendationRequestMutation,
  useSubmitRecommendationTurnFeedbackMutation,
} from './hooks';

vi.mock('@alcopilot/customer-api-client', () => ({
  submitRecommendationRequest: vi.fn(),
  submitRecommendationTurnFeedback: vi.fn(),
}));

describe('recommendation hooks', () => {
  it('invalidates summaries and the returned session after message submit', async () => {
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');
    vi.mocked(submitRecommendationRequest).mockResolvedValue({ sessionId: 'session-1' });

    const { result } = renderHook(() => useSubmitRecommendationRequestMutation(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      await result.current.mutateAsync({
        sessionId: null,
        message: 'Something citrusy',
      });
    });

    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: portalKeys.recommendationSessions,
    });
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: portalKeys.recommendationSession('session-1'),
    });
    expect(setQueryData).not.toHaveBeenCalled();
  });

  it('invalidates summaries and the current session after feedback submit', async () => {
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');
    vi.mocked(submitRecommendationTurnFeedback).mockResolvedValue(undefined);

    const { result } = renderHook(() => useSubmitRecommendationTurnFeedbackMutation(), {
      wrapper: createWrapper(queryClient),
    });

    await act(async () => {
      await result.current.mutateAsync({
        sessionId: 'session-1',
        turnId: 'turn-1',
        rating: 'positive',
      });
    });

    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: portalKeys.recommendationSessions,
    });
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: portalKeys.recommendationSession('session-1'),
    });
    expect(setQueryData).not.toHaveBeenCalled();
  });
});

function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

function createWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}
