import { listAuditLogEntries } from '@alcopilot/management-api-client';
import { useQuery } from '@tanstack/react-query';
import { portalKeys } from '@/lib/queryKeys';

export function useAuditLogEntries() {
  return useQuery({
    queryKey: portalKeys.audit,
    queryFn: listAuditLogEntries,
    refetchInterval: 15_000,
  });
}
