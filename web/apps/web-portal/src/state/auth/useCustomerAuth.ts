import { getCustomerSession, logoutCustomer } from '@alcopilot/customer-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function useCustomerSession() {
  return useQuery({
    queryKey: portalKeys.session,
    queryFn: getCustomerSession,
    retry: false,
  });
}

export function useLogoutCustomerMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logoutCustomer,
    onSuccess: async () => {
      await Promise.all([
        queryClient.removeQueries({ queryKey: portalKeys.profile }),
        queryClient.removeQueries({ queryKey: portalKeys.ingredients }),
        queryClient.removeQueries({ queryKey: portalKeys.recommendationSessions }),
      ]);
      await queryClient.invalidateQueries({ queryKey: portalKeys.session });
    },
  });
}
