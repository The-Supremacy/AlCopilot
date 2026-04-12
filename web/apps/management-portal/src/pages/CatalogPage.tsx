import { Link } from '@tanstack/react-router';
import { CupSoda, Tags, Wheat } from 'lucide-react';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';
import { CatalogShell } from '@/features/catalog/CatalogShell';
import { useDrinks, useIngredients, useTags } from '@/lib/usePortalData';

const sections = [
  {
    to: '/catalog/drinks',
    label: 'Drinks',
    description: 'Browse cocktails, review recipes, and open a separate create or edit form.',
    icon: CupSoda,
  },
  {
    to: '/catalog/tags',
    label: 'Tags',
    description: 'Manage classification labels through a dedicated list and form view.',
    icon: Tags,
  },
  {
    to: '/catalog/ingredients',
    label: 'Ingredients',
    description:
      'Review ingredient records separately from drinks and adjust brands with more clarity.',
    icon: Wheat,
  },
] as const;

export function CatalogPage() {
  const drinks = useDrinks();
  const tags = useTags();
  const ingredients = useIngredients();

  const counts = {
    drinks: drinks.data?.totalCount ?? 0,
    tags: tags.data?.length ?? 0,
    ingredients: ingredients.data?.length ?? 0,
  };

  return (
    <CatalogShell
      title="Catalog management"
      description="Choose a dedicated entity list view first, then create or edit records in a separate form surface."
    >
      <div className="grid gap-6 md:grid-cols-2">
        {sections.map((section) => {
          const Icon = section.icon;
          const count =
            section.label === 'Drinks'
              ? counts.drinks
              : section.label === 'Tags'
                ? counts.tags
                : counts.ingredients;

          return (
            <SectionCard
              key={section.to}
              title={section.label}
              description={section.description}
              action={
                <Button asChild variant="outline" size="sm">
                  <Link to={section.to}>Open</Link>
                </Button>
              }
            >
              <div className="flex items-center gap-4 rounded-2xl border border-border bg-background/80 p-4">
                <div className="rounded-2xl bg-primary/10 p-3 text-primary">
                  <Icon className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Current records</p>
                  <strong className="text-2xl font-semibold text-slate-950">{count}</strong>
                </div>
              </div>
            </SectionCard>
          );
        })}
      </div>
    </CatalogShell>
  );
}
