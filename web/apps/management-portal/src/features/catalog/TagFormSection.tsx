import type { FormEvent } from 'react';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

type TagFormSectionProps = {
  isEditing: boolean;
  name: string;
  onNameChange: (value: string) => void;
  onSubmit: () => Promise<void>;
  onDelete?: () => void;
  onCancel?: () => void;
};

export function TagFormSection({
  isEditing,
  name,
  onNameChange,
  onSubmit,
  onDelete,
  onCancel,
}: TagFormSectionProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <SectionCard
      title={isEditing ? 'Edit tag' : 'Create tag'}
      description="This form is separate from the list to keep navigation clean."
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <label className="space-y-2 text-sm font-medium">
          <span>Tag name</span>
          <Input
            value={name}
            onChange={(event) => onNameChange(event.target.value)}
            placeholder="Classic"
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
