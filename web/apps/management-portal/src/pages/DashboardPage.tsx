import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { useAuditLogEntries } from '@/features/audit/useAuditData';
import { useDrinks, useIngredients, useTags } from '@/features/catalog/api/hooks';
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
    <div className="space-y-5 sm:space-y-6">
      <section className="rounded-[24px] border border-border/70 bg-card/80 p-4 shadow-soft sm:p-6">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Overview</p>
        <div className="mt-3 flex flex-col gap-4 sm:mt-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-2xl space-y-3">
            <h1 className="font-display text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
              Review catalog health, recent imports, and mutation visibility in one place.
            </h1>
            <p className="text-sm leading-6 text-muted-foreground">
              Import sync remains polling-based and synchronous. Every mutating command now
              contributes to the audit log so operator review is not limited to domain event
              records.
            </p>
          </div>
          {latestBatch ? (
            <div className="w-full min-w-0 rounded-2xl border border-border bg-background/90 px-4 py-3 sm:w-auto sm:min-w-72 sm:max-w-sm">
              <p className="text-xs uppercase tracking-[0.26em] text-muted-foreground">
                Latest import
              </p>
              <div className="mt-2 flex min-w-0 flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-3">
                <strong className="min-w-0 text-sm leading-5">
                  {latestBatch.source.displayName || latestBatch.strategyKey}
                </strong>
                <StatusPill
                  className="max-w-full whitespace-normal text-center"
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

      <section className="grid gap-3 sm:gap-4 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => (
          <div
            key={metric.label}
            className="rounded-2xl border border-border/70 bg-card/85 p-4 shadow-soft sm:p-5"
          >
            <p className="text-sm text-muted-foreground">{metric.label}</p>
            <strong className="mt-2 block text-3xl font-semibold tracking-tight text-foreground sm:mt-3 sm:text-4xl">
              {metric.value}
            </strong>
          </div>
        ))}
      </section>

      <div className="grid gap-5 sm:gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <SectionCard
          title="Operator workflow"
          description="Current workflow validates on import start, keeps review optional, and still requires explicit apply rather than background sync jobs."
        >
          <ol className="grid gap-4 text-sm text-muted-foreground">
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3 leading-6">
              Start an import from the preserved snapshot and capture provenance metadata.
            </li>
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3 leading-6">
              Review diagnostics immediately, then open Review if you need row-level review
              visibility.
            </li>
            <li className="rounded-xl border border-border bg-background/80 px-4 py-3 leading-6">
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
              <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                <strong className="text-foreground">{latestAudit.summary}</strong>
                <StatusPill tone="neutral">{latestAudit.actor}</StatusPill>
              </div>
              <p className="break-words text-muted-foreground">
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
