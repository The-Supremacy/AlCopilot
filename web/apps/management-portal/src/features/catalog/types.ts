export type RecipeDraft = {
  ingredientId: string;
  quantity: string;
  recommendedBrand: string;
};

export type DrinkFormState = {
  name: string;
  category: string;
  description: string;
  method: string;
  garnish: string;
  imageUrl: string;
  tagIds: string[];
  recipeEntries: RecipeDraft[];
};

export const emptyDrinkForm: DrinkFormState = {
  name: '',
  category: '',
  description: '',
  method: '',
  garnish: '',
  imageUrl: '',
  tagIds: [],
  recipeEntries: [{ ingredientId: '', quantity: '', recommendedBrand: '' }],
};
