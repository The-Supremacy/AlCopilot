import type { LinkProps } from '@tanstack/react-router';

type BreadcrumbLinkTarget = Pick<LinkProps, 'to' | 'params'>;

export type BreadcrumbDefinition = {
  label: string;
  to?: BreadcrumbLinkTarget;
};

declare module '@tanstack/react-router' {
  interface StaticDataRouteOption {
    breadcrumbs?: BreadcrumbDefinition[];
  }
}

export function createBreadcrumb(label: string, to?: BreadcrumbLinkTarget): BreadcrumbDefinition {
  return { label, to };
}

export function catalogBreadcrumbs(...tail: BreadcrumbDefinition[]) {
  return [createBreadcrumb('Catalog', { to: '/catalog' }), ...tail];
}

export function drinksBreadcrumbs(...tail: BreadcrumbDefinition[]) {
  return [
    createBreadcrumb('Catalog', { to: '/catalog' }),
    createBreadcrumb('Drinks', { to: '/catalog/drinks' }),
    ...tail,
  ];
}

export function tagsBreadcrumbs(...tail: BreadcrumbDefinition[]) {
  return [
    createBreadcrumb('Catalog', { to: '/catalog' }),
    createBreadcrumb('Tags', { to: '/catalog/tags' }),
    ...tail,
  ];
}

export function ingredientsBreadcrumbs(...tail: BreadcrumbDefinition[]) {
  return [
    createBreadcrumb('Catalog', { to: '/catalog' }),
    createBreadcrumb('Ingredients', { to: '/catalog/ingredients' }),
    ...tail,
  ];
}
