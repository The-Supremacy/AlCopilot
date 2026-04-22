import { getManagementSession } from '@alcopilot/management-api-client';
import { useQuery } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function useManagementSession() {
  return useQuery({
    queryKey: portalKeys.session,
    queryFn: getManagementSession,
    retry: false,
  });
}
