import type { FormEvent } from 'react';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import type { DrinkFormState } from './types';

type TagOption = { id: string; name: string };
type IngredientOption = { id: string; name: string };

type DrinkEditorSectionProps = {
  isEditing: boolean;
  drinkForm: DrinkFormState;
  tags: TagOption[];
  ingredients: IngredientOption[];
  onDrinkFormChange: (updater: (current: DrinkFormState) => DrinkFormState) => void;
  onSubmit: () => Promise<void>;
  onDelete?: () => void;
  onCancel?: () => void;
};

export function DrinkEditorSection({
  isEditing,
  drinkForm,
  tags,
  ingredients,
  onDrinkFormChange,
  onSubmit,
  onDelete,
  onCancel,
}: DrinkEditorSectionProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <SectionCard
      title={isEditing ? 'Edit drink' : 'Create drink'}
      description="Use this dedicated form to manage recipes, tags, and presentation details."
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <div className="grid gap-4 md:grid-cols-2">
          <label className="space-y-2 text-sm font-medium">
            <span>Drink name</span>
            <Input
              value={drinkForm.name}
              onChange={(event) =>
                onDrinkFormChange((current) => ({ ...current, name: event.target.value }))
              }
            />
          </label>
          <label className="space-y-2 text-sm font-medium">
            <span>Drink category</span>
            <Input
              value={drinkForm.category}
              onChange={(event) =>
                onDrinkFormChange((current) => ({ ...current, category: event.target.value }))
              }
              placeholder="Contemporary Classics"
            />
          </label>
        </div>

        <label className="space-y-2 text-sm font-medium">
          <span>Image URL</span>
          <Input
            value={drinkForm.imageUrl}
            onChange={(event) =>
              onDrinkFormChange((current) => ({ ...current, imageUrl: event.target.value }))
            }
          />
        </label>

        <label className="space-y-2 text-sm font-medium">
          <span>Description</span>
          <Textarea
            rows={4}
            value={drinkForm.description}
            onChange={(event) =>
              onDrinkFormChange((current) => ({ ...current, description: event.target.value }))
            }
          />
        </label>

        <div className="grid gap-4 md:grid-cols-2">
          <label className="space-y-2 text-sm font-medium">
            <span>Method</span>
            <Textarea
              rows={4}
              value={drinkForm.method}
              onChange={(event) =>
                onDrinkFormChange((current) => ({ ...current, method: event.target.value }))
              }
              placeholder="Shake with ice"
            />
          </label>
          <label className="space-y-2 text-sm font-medium">
            <span>Garnish</span>
            <Textarea
              rows={4}
              value={drinkForm.garnish}
              onChange={(event) =>
                onDrinkFormChange((current) => ({ ...current, garnish: event.target.value }))
              }
              placeholder="Lime wheel"
            />
          </label>
        </div>

        <fieldset className="space-y-3">
          <legend className="text-sm font-semibold text-slate-950">Tags</legend>
          <div className="grid gap-3 md:grid-cols-2">
            {tags.map((tag) => (
              <label
                key={tag.id}
                className="flex items-center gap-3 rounded-xl border border-border bg-background/80 px-3 py-3 text-sm"
              >
                <input
                  type="checkbox"
                  checked={drinkForm.tagIds.includes(tag.id)}
                  onChange={(event) =>
                    onDrinkFormChange((current) => ({
                      ...current,
                      tagIds: event.target.checked
                        ? [...current.tagIds, tag.id]
                        : current.tagIds.filter((item) => item !== tag.id),
                    }))
                  }
                />
                <span>{tag.name}</span>
              </label>
            ))}
          </div>
        </fieldset>

        <div className="space-y-3">
          <div className="flex items-center justify-between gap-3">
            <h3 className="text-sm font-semibold text-slate-950">Recipe</h3>
            <Button
              variant="outline"
              size="sm"
              onClick={() =>
                onDrinkFormChange((current) => ({
                  ...current,
                  recipeEntries: [
                    ...current.recipeEntries,
                    { ingredientId: '', quantity: '', recommendedBrand: '' },
                  ],
                }))
              }
            >
              Add ingredient
            </Button>
          </div>
          {drinkForm.recipeEntries.map((entry, index) => (
            <div
              key={`${entry.ingredientId}-${index}`}
              className="grid gap-3 rounded-xl border border-border bg-background/80 p-3 md:grid-cols-[1.3fr_0.7fr_0.8fr_auto]"
            >
              <Select
                aria-label={`Recipe ingredient ${index + 1}`}
                value={entry.ingredientId}
                onChange={(event) =>
                  onDrinkFormChange((current) => ({
                    ...current,
                    recipeEntries: current.recipeEntries.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, ingredientId: event.target.value } : item,
                    ),
                  }))
                }
              >
                <option value="">Choose ingredient</option>
                {ingredients.map((ingredient) => (
                  <option key={ingredient.id} value={ingredient.id}>
                    {ingredient.name}
                  </option>
                ))}
              </Select>
              <Input
                aria-label={`Recipe quantity ${index + 1}`}
                value={entry.quantity}
                onChange={(event) =>
                  onDrinkFormChange((current) => ({
                    ...current,
                    recipeEntries: current.recipeEntries.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, quantity: event.target.value } : item,
                    ),
                  }))
                }
                placeholder="1 oz"
              />
              <Input
                aria-label={`Recommended brand ${index + 1}`}
                value={entry.recommendedBrand}
                onChange={(event) =>
                  onDrinkFormChange((current) => ({
                    ...current,
                    recipeEntries: current.recipeEntries.map((item, itemIndex) =>
                      itemIndex === index
                        ? { ...item, recommendedBrand: event.target.value }
                        : item,
                    ),
                  }))
                }
                placeholder="Recommended brand"
              />
              <Button
                variant="ghost"
                size="sm"
                onClick={() =>
                  onDrinkFormChange((current) => ({
                    ...current,
                    recipeEntries: current.recipeEntries.filter(
                      (_, itemIndex) => itemIndex !== index,
                    ),
                  }))
                }
              >
                Remove
              </Button>
            </div>
          ))}
        </div>

        <div className="flex flex-wrap gap-3">
          <Button type="submit">Save</Button>
          {onCancel ? (
            <Button variant="outline" type="button" onClick={onCancel}>
              Cancel
            </Button>
          ) : null}
          {isEditing && onDelete ? (
            <Button variant="destructive" type="button" onClick={onDelete}>
              Delete
            </Button>
          ) : null}
        </div>
      </form>
    </SectionCard>
  );
}
