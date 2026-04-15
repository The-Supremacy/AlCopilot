import {
  getCustomerProfile,
  listCustomerIngredients,
  saveCustomerProfile,
  type SaveCustomerProfileInput,
} from '@alcopilot/customer-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function useCustomerIngredients() {
  return useQuery({
    queryKey: portalKeys.ingredients,
    queryFn: listCustomerIngredients,
  });
}

export function useCustomerProfile() {
  return useQuery({
    queryKey: portalKeys.profile,
    queryFn: getCustomerProfile,
  });
}

export function useSaveCustomerProfileMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: SaveCustomerProfileInput) => saveCustomerProfile(input),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: portalKeys.profile }),
        queryClient.invalidateQueries({ queryKey: portalKeys.recommendationSessions }),
      ]);
    },
  });
}
