import type { ReactNode } from 'react';
type CatalogShellProps = {
  title: string;
  description: string;
  children: ReactNode;
};

export function CatalogShell({ title, description, children }: CatalogShellProps) {
  return (
    <div className="space-y-6">
      <header className="space-y-3">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Catalog</p>
        <div className="space-y-2">
          <h1 className="text-3xl font-semibold tracking-tight text-slate-950">{title}</h1>
          <p className="max-w-3xl text-sm leading-6 text-muted-foreground">{description}</p>
        </div>
      </header>

      {children}
    </div>
  );
}
