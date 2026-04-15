import {
  createDrink,
  createIngredient,
  createTag,
  deleteDrink,
  deleteIngredient,
  deleteTag,
  updateDrink,
  updateIngredient,
  updateTag,
  type CreateDrinkInput,
  type CreateIngredientInput,
  type CreateTagInput,
  type UpdateDrinkInput,
  type UpdateIngredientInput,
} from '@alcopilot/management-api-client';
import { mutationOptions, type QueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

async function invalidateCatalog(queryClient: QueryClient) {
  await Promise.all([
    queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
    queryClient.invalidateQueries({ queryKey: portalKeys.tags }),
    queryClient.invalidateQueries({ queryKey: portalKeys.ingredients }),
    queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
  ]);
}

async function invalidateDrink(queryClient: QueryClient, id?: string) {
  await Promise.all([
    queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
    id ? queryClient.invalidateQueries({ queryKey: portalKeys.drink(id) }) : Promise.resolve(),
    queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
  ]);
}

export function createTagMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: (input: CreateTagInput) => createTag(input),
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function updateTagMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: ({ id, input }: { id: string; input: CreateTagInput }) => updateTag(id, input),
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function deleteTagMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: deleteTag,
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function createIngredientMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: (input: CreateIngredientInput) => createIngredient(input),
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function updateIngredientMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: ({ id, input }: { id: string; input: UpdateIngredientInput }) =>
      updateIngredient(id, input),
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function deleteIngredientMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: deleteIngredient,
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function createDrinkMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: (input: CreateDrinkInput) => createDrink(input),
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}

export function updateDrinkMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: ({ id, input }: { id: string; input: UpdateDrinkInput }) => updateDrink(id, input),
    onSuccess: async (_data, variables) => invalidateDrink(queryClient, variables.id),
  });
}

export function deleteDrinkMutationOptions(queryClient: QueryClient) {
  return mutationOptions({
    mutationFn: deleteDrink,
    onSuccess: async () => invalidateCatalog(queryClient),
  });
}
