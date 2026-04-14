import { getDrink, listDrinks, listIngredients, listTags } from '@alcopilot/management-api-client';
import { queryOptions } from '@tanstack/react-query';
import { portalKeys } from '@/state/queryKeys';

export function drinksQueryOptions() {
  return queryOptions({
    queryKey: portalKeys.drinks,
    queryFn: () => listDrinks(),
  });
}

export function drinkQueryOptions(id: string) {
  return queryOptions({
    queryKey: portalKeys.drink(id),
    queryFn: () => getDrink(id),
  });
}

export function tagsQueryOptions() {
  return queryOptions({
    queryKey: portalKeys.tags,
    queryFn: () => listTags(),
  });
}

export function ingredientsQueryOptions() {
  return queryOptions({
    queryKey: portalKeys.ingredients,
    queryFn: () => listIngredients(),
  });
}
