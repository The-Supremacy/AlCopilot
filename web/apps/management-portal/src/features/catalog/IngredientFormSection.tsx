import type { FormEvent } from 'react';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

type IngredientFormSectionProps = {
  isEditing: boolean;
  name: string;
  ingredientGroup: string;
  notableBrands: string;
  onNameChange: (value: string) => void;
  onIngredientGroupChange: (value: string) => void;
  onNotableBrandsChange: (value: string) => void;
  onSubmit: () => Promise<void>;
  onDelete?: () => void;
  onCancel?: () => void;
};

export function IngredientFormSection({
  isEditing,
  name,
  ingredientGroup,
  notableBrands,
  onNameChange,
  onIngredientGroupChange,
  onNotableBrandsChange,
  onSubmit,
  onDelete,
  onCancel,
}: IngredientFormSectionProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <SectionCard
      title={isEditing ? 'Edit ingredient' : 'Create ingredient'}
      description="Use a dedicated form to manage ingredient details."
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <label className="space-y-2 text-sm font-medium">
          <span>Ingredient name</span>
          <Input
            value={name}
            onChange={(event) => onNameChange(event.target.value)}
            placeholder="Gin"
          />
        </label>

        <label className="space-y-2 text-sm font-medium">
          <span>Notable brands</span>
          <Input
            value={notableBrands}
            onChange={(event) => onNotableBrandsChange(event.target.value)}
            placeholder="Tanqueray, Beefeater"
          />
        </label>

        <label className="space-y-2 text-sm font-medium">
          <span>Ingredient group</span>
          <Input
            value={ingredientGroup}
            onChange={(event) => onIngredientGroupChange(event.target.value)}
            placeholder="Gin"
          />
        </label>

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
