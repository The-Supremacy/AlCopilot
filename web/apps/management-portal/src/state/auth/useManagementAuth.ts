import { logoutManagement, getManagementSession } from '@alcopilot/management-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function useManagementSession() {
  return useQuery({
    queryKey: portalKeys.session,
    queryFn: getManagementSession,
    retry: false,
  });
}

export function useLogoutManagementMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logoutManagement,
    onSuccess: async () => {
      await Promise.all([
        queryClient.removeQueries({ queryKey: portalKeys.drinks }),
        queryClient.removeQueries({ queryKey: ['drink'] }),
        queryClient.removeQueries({ queryKey: portalKeys.tags }),
        queryClient.removeQueries({ queryKey: portalKeys.ingredients }),
        queryClient.removeQueries({ queryKey: portalKeys.imports }),
        queryClient.removeQueries({ queryKey: ['import-batch'] }),
        queryClient.removeQueries({ queryKey: portalKeys.audit }),
      ]);
      await queryClient.invalidateQueries({ queryKey: portalKeys.session });
    },
  });
}
