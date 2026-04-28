import {
  getRecommendationSession,
  listRecommendationSessions,
  submitRecommendationRequest,
  submitRecommendationTurnFeedback,
  type SubmitRecommendationRequestInput,
  type SubmitRecommendationTurnFeedbackInput,
} from '@alcopilot/customer-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function useRecommendationSessions() {
  return useQuery({
    queryKey: portalKeys.recommendationSessions,
    queryFn: listRecommendationSessions,
  });
}

export function useRecommendationSession(sessionId: string | undefined) {
  return useQuery({
    queryKey: portalKeys.recommendationSession(sessionId ?? 'pending'),
    queryFn: () => getRecommendationSession(sessionId!),
    enabled: Boolean(sessionId),
  });
}

export function useSubmitRecommendationRequestMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SubmitRecommendationRequestInput) => submitRecommendationRequest(input),
    onSuccess: async (result) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: portalKeys.recommendationSessions }),
        queryClient.invalidateQueries({
          queryKey: portalKeys.recommendationSession(result.sessionId),
        }),
      ]);
    },
  });
}

export function useSubmitRecommendationTurnFeedbackMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SubmitRecommendationTurnFeedbackInput) =>
      submitRecommendationTurnFeedback(input),
    onSuccess: async (_result, input) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: portalKeys.recommendationSessions }),
        queryClient.invalidateQueries({
          queryKey: portalKeys.recommendationSession(input.sessionId),
        }),
      ]);
    },
  });
}
