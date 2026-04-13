import {
  createDrink,
  createIngredient,
  createTag,
  deleteDrink,
  deleteIngredient,
  deleteTag,
  getDrink,
  listDrinks,
  listIngredients,
  listTags,
  updateDrink,
  updateIngredient,
  updateTag,
  type CreateDrinkInput,
  type CreateIngredientInput,
  type CreateTagInput,
  type UpdateDrinkInput,
  type UpdateIngredientInput,
} from '@alcopilot/management-api-client';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalKeys } from '@/lib/queryKeys';

function useInvalidateCatalogData() {
  const queryClient = useQueryClient();

  async function invalidateCatalog() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
      queryClient.invalidateQueries({ queryKey: portalKeys.tags }),
      queryClient.invalidateQueries({ queryKey: portalKeys.ingredients }),
      queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
    ]);
  }

  async function invalidateDrink(id?: string) {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: portalKeys.drinks }),
      id ? queryClient.invalidateQueries({ queryKey: portalKeys.drink(id) }) : Promise.resolve(),
      queryClient.invalidateQueries({ queryKey: portalKeys.audit }),
    ]);
  }

  return { invalidateCatalog, invalidateDrink };
}

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
    queryFn: () => listTags(),
  });
}

export function useIngredients() {
  return useQuery({
    queryKey: portalKeys.ingredients,
    queryFn: () => listIngredients(),
  });
}

export function useCreateTagMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: (input: CreateTagInput) => createTag(input),
    onSuccess: invalidateCatalog,
  });
}

export function useUpdateTagMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: CreateTagInput }) => updateTag(id, input),
    onSuccess: invalidateCatalog,
  });
}

export function useDeleteTagMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: deleteTag,
    onSuccess: invalidateCatalog,
  });
}

export function useCreateIngredientMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: (input: CreateIngredientInput) => createIngredient(input),
    onSuccess: invalidateCatalog,
  });
}

export function useUpdateIngredientMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateIngredientInput }) =>
      updateIngredient(id, input),
    onSuccess: invalidateCatalog,
  });
}

export function useDeleteIngredientMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: deleteIngredient,
    onSuccess: invalidateCatalog,
  });
}

export function useCreateDrinkMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: (input: CreateDrinkInput) => createDrink(input),
    onSuccess: invalidateCatalog,
  });
}

export function useUpdateDrinkMutation() {
  const { invalidateDrink } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateDrinkInput }) => updateDrink(id, input),
    onSuccess: async (_data, variables) => invalidateDrink(variables.id),
  });
}

export function useDeleteDrinkMutation() {
  const { invalidateCatalog } = useInvalidateCatalogData();

  return useMutation({
    mutationFn: deleteDrink,
    onSuccess: invalidateCatalog,
  });
}
