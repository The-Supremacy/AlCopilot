import { Link, useMatches } from '@tanstack/react-router';
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import type { BreadcrumbDefinition } from './breadcrumbs';

export function PortalBreadcrumbs() {
  const breadcrumbs =
    useMatches({
      select: (matches) => matches.at(-1)?.staticData.breadcrumbs ?? [],
    }) ?? [];

  if (breadcrumbs.length < 2) {
    return null;
  }

  return (
    <Breadcrumb>
      <BreadcrumbList>
        {breadcrumbs.map((breadcrumb: BreadcrumbDefinition, index: number) => {
          const isLast = index === breadcrumbs.length - 1;

          return (
            <FragmentedBreadcrumb
              key={`${breadcrumb.label}-${index}`}
              breadcrumb={breadcrumb}
              isLast={isLast}
            />
          );
        })}
      </BreadcrumbList>
    </Breadcrumb>
  );
}

function FragmentedBreadcrumb({
  breadcrumb,
  isLast,
}: {
  breadcrumb: BreadcrumbDefinition;
  isLast: boolean;
}) {
  return (
    <>
      <BreadcrumbItem>
        {isLast || !breadcrumb.to ? (
          <BreadcrumbPage>{breadcrumb.label}</BreadcrumbPage>
        ) : (
          <BreadcrumbLink asChild>
            <Link to={breadcrumb.to.to} params={breadcrumb.to.params}>
              {breadcrumb.label}
            </Link>
          </BreadcrumbLink>
        )}
      </BreadcrumbItem>
      {isLast ? null : <BreadcrumbSeparator />}
    </>
  );
}
