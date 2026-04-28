import { useEffect, useState } from 'react';
import type { IngredientDto } from '@alcopilot/customer-api-client';
import { Search } from 'lucide-react';
import { InlineMessage } from '@/components/InlineMessage';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
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
  const [showAll, setShowAll] = useState(false);
  const [pendingGroupSelection, setPendingGroupSelection] = useState<IngredientDto | null>(null);

  useEffect(() => {
    setLocalSelection(selectedIds);
  }, [selectedIds]);

  const normalizedSearch = search.trim().toLowerCase();
  const filteredIngredients = ingredients.filter((ingredient) =>
    ingredient.name.toLowerCase().includes(normalizedSearch),
  );
  const selectedIngredients = ingredients.filter((ingredient) =>
    localSelection.includes(ingredient.id),
  );
  const visibleIngredients =
    normalizedSearch.length > 0 ? filteredIngredients : showAll ? ingredients : selectedIngredients;
  const isSearchIdle = normalizedSearch.length === 0;

  function toggleIngredient(id: string, checked: boolean) {
    const ingredient = ingredients.find((item) => item.id === id);
    if (checked && ingredient?.ingredientGroup) {
      const groupIngredients = getGroupIngredients(ingredient.ingredientGroup);
      const hasUnselectedGroupIngredient = groupIngredients.some(
        (item) => !localSelection.includes(item.id),
      );
      if (groupIngredients.length > 1 && hasUnselectedGroupIngredient) {
        setPendingGroupSelection(ingredient);
        return;
      }
    }

    const nextIds = checked
      ? [...new Set([...localSelection, id])]
      : localSelection.filter((selectedId) => selectedId !== id);

    applySelection(nextIds);
  }

  function clearSelection() {
    applySelection([]);
  }

  function applySelection(nextIds: string[]) {
    setLocalSelection(nextIds);
    onChange(nextIds);
  }

  function getGroupIngredients(group: string) {
    return ingredients.filter((ingredient) => ingredient.ingredientGroup === group);
  }

  function applyPendingSingleSelection() {
    if (!pendingGroupSelection) {
      return;
    }

    applySelection([...new Set([...localSelection, pendingGroupSelection.id])]);
    setPendingGroupSelection(null);
  }

  function applyPendingGroupSelection() {
    if (!pendingGroupSelection?.ingredientGroup) {
      return;
    }

    const groupIds = getGroupIngredients(pendingGroupSelection.ingredientGroup).map(
      (ingredient) => ingredient.id,
    );
    applySelection([...new Set([...localSelection, ...groupIds])]);
    setPendingGroupSelection(null);
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
          placeholder="Start typing to search ingredients"
          className="pl-9"
          aria-label={`Search ${title}`}
        />
      </div>

      {isSearchIdle && !showAll ? (
        <div className="flex flex-col gap-3 rounded-2xl border border-dashed border-border/70 bg-background/65 p-4 text-sm text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
          <p>
            Start typing to browse ingredients. Selected items stay visible here so you can review
            them quickly.
          </p>
          <Button variant="outline" size="sm" onClick={() => setShowAll(true)}>
            Browse full list
          </Button>
        </div>
      ) : null}

      {!isSearchIdle && filteredIngredients.length === 0 ? (
        <InlineMessage tone="warning" message="No ingredients match that search yet." />
      ) : (
        <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {visibleIngredients.map((ingredient) => {
            const checked = localSelection.includes(ingredient.id);
            return (
              <label
                key={ingredient.id}
                className={cn(
                  'grid cursor-pointer grid-cols-[auto_minmax(0,1fr)] items-center gap-3 rounded-2xl border border-border/70 bg-card/80 p-4 transition hover:border-primary/40 hover:bg-card',
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
                    {[
                      ingredient.ingredientGroup ? `${ingredient.ingredientGroup} group` : null,
                      ingredient.notableBrands.length > 0
                        ? ingredient.notableBrands.join(', ')
                        : 'No notable brands saved yet',
                    ]
                      .filter(Boolean)
                      .join(' · ')}
                  </span>
                </span>
              </label>
            );
          })}
        </div>
      )}

      {isSearchIdle && showAll && ingredients.length > 0 ? (
        <div className="flex justify-end">
          <Button variant="ghost" size="sm" onClick={() => setShowAll(false)}>
            Show selected only
          </Button>
        </div>
      ) : null}

      {localSelection.length === 0 ? (
        <InlineMessage tone="warning" message={emptySelectionMessage} />
      ) : null}

      <AlertDialog
        open={pendingGroupSelection !== null}
        onOpenChange={(open) => {
          if (!open) {
            setPendingGroupSelection(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              Select all {pendingGroupSelection?.ingredientGroup} ingredients?
            </AlertDialogTitle>
            <AlertDialogDescription>
              {pendingGroupSelection
                ? `${pendingGroupSelection.name} belongs to the ${pendingGroupSelection.ingredientGroup} group. You can select only this ingredient or mark every ${pendingGroupSelection.ingredientGroup} ingredient in the list.`
                : null}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel asChild>
              <Button variant="outline" onClick={applyPendingSingleSelection}>
                Only {pendingGroupSelection?.name}
              </Button>
            </AlertDialogCancel>
            <AlertDialogAction asChild>
              <Button onClick={applyPendingGroupSelection}>
                All {pendingGroupSelection?.ingredientGroup} ingredients
              </Button>
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
