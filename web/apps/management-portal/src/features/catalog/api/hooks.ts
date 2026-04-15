import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createDrinkMutationOptions,
  createIngredientMutationOptions,
  createTagMutationOptions,
  deleteDrinkMutationOptions,
  deleteIngredientMutationOptions,
  deleteTagMutationOptions,
  updateDrinkMutationOptions,
  updateIngredientMutationOptions,
  updateTagMutationOptions,
} from './mutation-options';
import {
  drinkQueryOptions,
  drinksQueryOptions,
  ingredientsQueryOptions,
  tagsQueryOptions,
} from './query-options';

export function useDrinks() {
  return useQuery(drinksQueryOptions());
}

export function useDrink(id: string | null) {
  return useQuery({
    ...(id ? drinkQueryOptions(id) : drinkQueryOptions('empty')),
    enabled: Boolean(id),
  });
}

export function useTags() {
  return useQuery(tagsQueryOptions());
}

export function useIngredients() {
  return useQuery(ingredientsQueryOptions());
}

export function useCreateTagMutation() {
  const queryClient = useQueryClient();

  return useMutation(createTagMutationOptions(queryClient));
}

export function useUpdateTagMutation() {
  const queryClient = useQueryClient();

  return useMutation(updateTagMutationOptions(queryClient));
}

export function useDeleteTagMutation() {
  const queryClient = useQueryClient();

  return useMutation(deleteTagMutationOptions(queryClient));
}

export function useCreateIngredientMutation() {
  const queryClient = useQueryClient();

  return useMutation(createIngredientMutationOptions(queryClient));
}

export function useUpdateIngredientMutation() {
  const queryClient = useQueryClient();

  return useMutation(updateIngredientMutationOptions(queryClient));
}

export function useDeleteIngredientMutation() {
  const queryClient = useQueryClient();

  return useMutation(deleteIngredientMutationOptions(queryClient));
}

export function useCreateDrinkMutation() {
  const queryClient = useQueryClient();

  return useMutation(createDrinkMutationOptions(queryClient));
}

export function useUpdateDrinkMutation() {
  const queryClient = useQueryClient();

  return useMutation(updateDrinkMutationOptions(queryClient));
}

export function useDeleteDrinkMutation() {
  const queryClient = useQueryClient();

  return useMutation(deleteDrinkMutationOptions(queryClient));
}
