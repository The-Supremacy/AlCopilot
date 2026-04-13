import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { useAuditLogEntries } from '@/features/audit/useAuditData';
import { useDrinks, useIngredients, useTags } from '@/features/catalog/useCatalogData';
import { useImportHistory } from '@/features/imports/useImportData';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';

export function DashboardPage() {
  const drinks = useDrinks();
  const tags = useTags();
  const ingredients = useIngredients();
  const imports = useImportHistory();
  const auditLog = useAuditLogEntries();

  const latestBatch = imports.data?.[0] ?? null;
  const latestAudit = auditLog.data?.[0] ?? null;

  const metrics = [
    { label: 'Drinks', value: drinks.data?.totalCount ?? 0 },
    { label: 'Tags', value: tags.data?.length ?? 0 },
    { label: 'Ingredients', value: ingredients.data?.length ?? 0 },
  ];

  return (
    <div className="space-y-6">
      <section className="rounded-[24px] border border-border/70 bg-card/80 p-6 shadow-soft">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Overview</p>
        <div className="mt-4 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-2xl space-y-3">
            <h1 className="font-display text-3xl font-semibold tracking-tight text-foreground">
              Review catalog health, recent imports, and mutation visibility in one place.
            </h1>
            <p className="text-sm leading-6 text-muted-foreground">
              Import sync remains polling-based and synchronous. Every mutating command now
              contributes to the audit log so operator review is not limited to domain event
              records.
            </p>
          </div>
          {latestBatch ? (
            <div className="rounded-2xl border border-border bg-background/90 px-4 py-3">
              <p className="text-xs uppercase tracking-[0.26em] text-muted-foreground">
                Latest import
              </p>
              <div className="mt-2 flex items-center gap-3">
                <strong className="text-sm">
                  {latestBatch.source.displayName || latestBatch.strategyKey}
                </strong>
                <StatusPill
                  tone={
                    latestBatch.status === 'Completed'
                      ? 'success'
                      : latestBatch.status === 'Cancelled'
                        ? 'neutral'
                        : 'warning'
                  }
                >
                  {formatImportBatchStatus(latestBatch.status)}
                </StatusPill>
              </div>
              <p className="mt-1 text-sm text-muted-foreground">
                {formatTimestamp(latestBatch.lastUpdatedAtUtc)}
              </p>
            </div>
          ) : null}
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => (
          <div
            key={metric.label}
            className="rounded-2xl border border-border/70 bg-card/85 p-5 shadow-soft"
          >
            <p className="text-sm text-muted-foreground">{metric.label}</p>
            <strong className="mt-3 block text-4xl font-semibold tracking-tight text-foreground">
              {metric.value}
            </strong>
          </div>
        ))}
      </section>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <SectionCard
          title="Operator workflow"
          description="Current workflow validates on import start, keeps review optional, and still requires explicit apply rather than background sync jobs."
        >
          <ol className="grid gap-4 text-sm text-muted-foreground">
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3">
              Start an import from the preserved snapshot and capture provenance metadata.
            </li>
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3">
              Review diagnostics immediately, then open Review if you need row-level review
              visibility.
            </li>
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3">
              Apply intentionally, then review the batch and audit history through persisted reads.
            </li>
          </ol>
        </SectionCard>

        <SectionCard
          title="Latest audit entry"
          description="Audit logging is generic and command-oriented, not inferred from domain events."
        >
          {latestAudit ? (
            <div className="space-y-3 rounded-xl border border-border bg-background/80 p-4 text-sm">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-foreground">{latestAudit.summary}</strong>
                <StatusPill tone="neutral">{latestAudit.actor}</StatusPill>
              </div>
              <p className="text-muted-foreground">
                {latestAudit.action} · {latestAudit.subjectType}
                {latestAudit.subjectKey ? ` · ${latestAudit.subjectKey}` : ''}
              </p>
              <p className="text-muted-foreground">{formatTimestamp(latestAudit.occurredAtUtc)}</p>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">
              No audit entries have been recorded yet.
            </p>
          )}
        </SectionCard>
      </div>
    </div>
  );
}
