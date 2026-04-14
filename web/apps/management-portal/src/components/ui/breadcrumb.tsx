import { Slot } from '@radix-ui/react-slot';
import type { ComponentPropsWithoutRef, HTMLAttributes, OlHTMLAttributes } from 'react';
import { ChevronRight, MoreHorizontal } from 'lucide-react';
import { cn } from '@/lib/utils';

export function Breadcrumb({ ...props }: ComponentPropsWithoutRef<'nav'>) {
  return <nav aria-label="Breadcrumb" {...props} />;
}

export function BreadcrumbList({ className, ...props }: OlHTMLAttributes<HTMLOListElement>) {
  return (
    <ol
      className={cn(
        'flex flex-wrap items-center gap-1.5 break-words text-sm text-muted-foreground sm:gap-2.5',
        className,
      )}
      {...props}
    />
  );
}

export function BreadcrumbItem({ className, ...props }: HTMLAttributes<HTMLLIElement>) {
  return <li className={cn('inline-flex items-center gap-1.5', className)} {...props} />;
}

export function BreadcrumbLink({
  asChild = false,
  className,
  ...props
}: ComponentPropsWithoutRef<'a'> & { asChild?: boolean }) {
  const Comp = asChild ? Slot : 'a';

  return (
    <Comp
      className={cn(
        'transition-colors hover:text-foreground focus-visible:outline-none',
        className,
      )}
      {...props}
    />
  );
}

export function BreadcrumbPage({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return (
    <span aria-current="page" className={cn('font-medium text-foreground', className)} {...props} />
  );
}

export function BreadcrumbSeparator({
  children,
  className,
  ...props
}: HTMLAttributes<HTMLLIElement>) {
  return (
    <li
      aria-hidden="true"
      className={cn('[&>svg]:size-3.5', className)}
      role="presentation"
      {...props}
    >
      {children ?? <ChevronRight />}
    </li>
  );
}

export function BreadcrumbEllipsis({ className, ...props }: HTMLAttributes<HTMLSpanElement>) {
  return (
    <span
      aria-hidden="true"
      className={cn('flex h-9 w-9 items-center justify-center', className)}
      role="presentation"
      {...props}
    >
      <MoreHorizontal className="h-4 w-4" />
      <span className="sr-only">More</span>
    </span>
  );
}
