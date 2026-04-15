import {
  applyImportBatch,
  cancelImportBatch,
  getImportBatch,
  listImportHistory,
  reviewImportBatch,
  startImport,
  type ApplyImportBatchInput,
  type StartImportInput,
} from '@alcopilot/management-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

function useInvalidateImportData() {
  const queryClient = useQueryClient();

  async function invalidateImports() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: portalKeys.imports }),
      queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
    ]);
  }

  return { invalidateImports };
}

export function useImportHistory() {
  return useQuery({
    queryKey: portalKeys.imports,
    queryFn: listImportHistory,
  });
}

export function useImportBatch(id: string | null) {
  return useQuery({
    queryKey: id ? portalKeys.importBatch(id) : ['import-batch', 'empty'],
    queryFn: () => getImportBatch(id!),
    enabled: Boolean(id),
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === 'InProgress' ? 15_000 : false;
    },
  });
}

export function useStartImportMutation() {
  const { invalidateImports } = useInvalidateImportData();

  return useMutation({
    mutationFn: (input: StartImportInput) => startImport(input),
    onSuccess: invalidateImports,
  });
}

export function useReviewImportBatchMutation() {
  const queryClient = useQueryClient();
  const { invalidateImports } = useInvalidateImportData();

  return useMutation({
    mutationFn: reviewImportBatch,
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await invalidateImports();
    },
  });
}

export function useCancelImportBatchMutation() {
  const queryClient = useQueryClient();
  const { invalidateImports } = useInvalidateImportData();

  return useMutation({
    mutationFn: cancelImportBatch,
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await invalidateImports();
    },
  });
}

export function useApplyImportBatchMutation() {
  const queryClient = useQueryClient();
  const { invalidateImports } = useInvalidateImportData();

  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ApplyImportBatchInput }) =>
      applyImportBatch(id, input),
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
        queryClient.invalidateQueries({ queryKey: portalKeys.tags }),
        queryClient.invalidateQueries({ queryKey: portalKeys.ingredients }),
        invalidateImports(),
      ]);
    },
  });
}
