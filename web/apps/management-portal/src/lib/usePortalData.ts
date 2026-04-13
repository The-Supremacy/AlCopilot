import {
  applyImportBatch,
  cancelImportBatch,
  createDrink,
  createIngredient,
  createTag,
  deleteDrink,
  deleteIngredient,
  deleteTag,
  getDrink,
  getImportBatch,
  listAuditLogEntries,
  listDrinks,
  listImportHistory,
  listIngredients,
  listTags,
  reviewImportBatch,
  startImport,
  updateDrink,
  updateIngredient,
  updateTag,
  type ApplyImportBatchInput,
  type CreateDrinkInput,
  type CreateIngredientInput,
  type CreateTagInput,
  type StartImportInput,
  type UpdateDrinkInput,
  type UpdateIngredientInput,
} from '@alcopilot/management-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

export const portalKeys = {
  session: ['management-session'] as const,
  drinks: ['drinks'] as const,
  drink: (id: string) => ['drink', id] as const,
  tags: ['tags'] as const,
  ingredients: ['ingredients'] as const,
  imports: ['imports'] as const,
  importBatch: (id: string) => ['import-batch', id] as const,
  audit: ['audit-log'] as const,
};

export function useDrinks() {
  return useQuery({
    queryKey: portalKeys.drinks,
    queryFn: () => listDrinks(),
  });
}

export function useDrink(id: string | null) {
  return useQuery({
    queryKey: id ? portalKeys.drink(id) : ['drink', 'empty'],
    queryFn: () => getDrink(id!),
    enabled: Boolean(id),
  });
}

export function useTags() {
  return useQuery({
    queryKey: portalKeys.tags,
    queryFn: listTags,
  });
}

export function useIngredients() {
  return useQuery({
    queryKey: portalKeys.ingredients,
    queryFn: () => listIngredients(),
  });
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

export function useAuditLogEntries() {
  return useQuery({
    queryKey: portalKeys.audit,
    queryFn: listAuditLogEntries,
    refetchInterval: 15_000,
  });
}

function useInvalidatePortalData() {
  const queryClient = useQueryClient();

  return async function invalidatePortalData() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
      queryClient.invalidateQueries({ queryKey: portalKeys.tags }),
      queryClient.invalidateQueries({ queryKey: portalKeys.ingredients }),
      queryClient.invalidateQueries({ queryKey: portalKeys.imports }),
      queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
    ]);
  };
}

export function useCreateTagMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: (input: CreateTagInput) => createTag(input),
    onSuccess: invalidate,
  });
}

export function useUpdateTagMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: CreateTagInput }) => updateTag(id, input),
    onSuccess: invalidate,
  });
}

export function useDeleteTagMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: deleteTag,
    onSuccess: invalidate,
  });
}

export function useCreateIngredientMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: (input: CreateIngredientInput) => createIngredient(input),
    onSuccess: invalidate,
  });
}

export function useUpdateIngredientMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateIngredientInput }) =>
      updateIngredient(id, input),
    onSuccess: invalidate,
  });
}

export function useDeleteIngredientMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: deleteIngredient,
    onSuccess: invalidate,
  });
}

export function useCreateDrinkMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: (input: CreateDrinkInput) => createDrink(input),
    onSuccess: invalidate,
  });
}

export function useUpdateDrinkMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateDrinkInput }) => updateDrink(id, input),
    onSuccess: invalidate,
  });
}

export function useDeleteDrinkMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: deleteDrink,
    onSuccess: invalidate,
  });
}

export function useStartImportMutation() {
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: (input: StartImportInput) => startImport(input),
    onSuccess: invalidate,
  });
}

export function useReviewImportBatchMutation() {
  const queryClient = useQueryClient();
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: reviewImportBatch,
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await invalidate();
    },
  });
}

export function useCancelImportBatchMutation() {
  const queryClient = useQueryClient();
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: cancelImportBatch,
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await invalidate();
    },
  });
}

export function useApplyImportBatchMutation() {
  const queryClient = useQueryClient();
  const invalidate = useInvalidatePortalData();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: ApplyImportBatchInput }) =>
      applyImportBatch(id, input),
    onSuccess: async (batch) => {
      queryClient.setQueryData(portalKeys.importBatch(batch.id), batch);
      await invalidate();
    },
  });
}
