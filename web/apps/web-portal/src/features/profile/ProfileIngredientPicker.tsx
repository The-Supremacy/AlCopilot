import { useEffect, useState } from 'react';
import type { IngredientDto } from '@alcopilot/customer-api-client';
import { Search } from 'lucide-react';
import { InlineMessage } from '@/components/InlineMessage';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

type ProfileIngredientPickerProps = {
  title: string;
  description: string;
  ingredients: IngredientDto[];
  selectedIds: string[];
  onChange: (nextIds: string[]) => void;
  emptySelectionMessage: string;
};

export function ProfileIngredientPicker({
  title,
  description,
  ingredients,
  selectedIds,
  onChange,
  emptySelectionMessage,
}: ProfileIngredientPickerProps) {
  const [search, setSearch] = useState('');
  const [localSelection, setLocalSelection] = useState<string[]>(selectedIds);

  useEffect(() => {
    setLocalSelection(selectedIds);
  }, [selectedIds]);

  const normalizedSearch = search.trim().toLowerCase();
  const filteredIngredients = ingredients.filter((ingredient) =>
    ingredient.name.toLowerCase().includes(normalizedSearch),
  );

  function toggleIngredient(id: string, checked: boolean) {
    const nextIds = checked
      ? [...new Set([...localSelection, id])]
      : localSelection.filter((selectedId) => selectedId !== id);

    setLocalSelection(nextIds);
    onChange(nextIds);
  }

  function clearSelection() {
    setLocalSelection([]);
    onChange([]);
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 rounded-2xl border border-border/70 bg-background/70 p-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h3 className="font-display text-lg text-foreground">{title}</h3>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary">{localSelection.length} selected</Badge>
          <Button
            variant="ghost"
            size="sm"
            onClick={clearSelection}
            disabled={localSelection.length === 0}
          >
            Clear
          </Button>
        </div>
      </div>

      <div className="relative">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search ingredients"
          className="pl-9"
          aria-label={`Search ${title}`}
        />
      </div>

      {filteredIngredients.length === 0 ? (
        <InlineMessage tone="warning" message="No ingredients match that search yet." />
      ) : (
        <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {filteredIngredients.map((ingredient) => {
            const checked = localSelection.includes(ingredient.id);
            return (
              <label
                key={ingredient.id}
                className={cn(
                  'flex cursor-pointer items-start gap-3 rounded-2xl border border-border/70 bg-card/80 p-4 transition hover:border-primary/40 hover:bg-card',
                  checked && 'border-primary/60 bg-primary/5',
                )}
              >
                <Checkbox
                  checked={checked}
                  onCheckedChange={(value) => toggleIngredient(ingredient.id, value === true)}
                  aria-label={`Select ${ingredient.name}`}
                />
                <span className="space-y-1">
                  <span className="block text-sm font-medium text-foreground">
                    {ingredient.name}
                  </span>
                  <span className="block text-xs text-muted-foreground">
                    {ingredient.notableBrands.length > 0
                      ? ingredient.notableBrands.join(', ')
                      : 'No notable brands saved yet'}
                  </span>
                </span>
              </label>
            );
          })}
        </div>
      )}

      {localSelection.length === 0 ? (
        <InlineMessage tone="warning" message={emptySelectionMessage} />
      ) : null}
    </div>
  );
}
