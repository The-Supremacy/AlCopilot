import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { useAuditLogEntries } from '@/features/audit/useAuditData';
import { useImportHistory } from '@/features/imports/useImportData';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';

export function AuditPage() {
  const auditLog = useAuditLogEntries();
  const imports = useImportHistory();

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Audit</p>
        <h1 className="font-display text-3xl font-semibold tracking-tight text-foreground">
          Review command history and import batch outcomes
        </h1>
      </header>

      <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
        <SectionCard
          title="Audit log"
          description="Recent successful mutating commands across catalog curation and import workflow."
        >
          <div className="space-y-3">
            {auditLog.data?.map((entry) => (
              <article
                key={entry.id}
                className="rounded-xl border border-border bg-background/80 p-4"
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                  <div className="space-y-1">
                    <strong className="text-sm text-foreground">{entry.summary}</strong>
                    <p className="text-sm text-muted-foreground">
                      {entry.action} · {entry.subjectType}
                      {entry.subjectKey ? ` · ${entry.subjectKey}` : ''}
                    </p>
                  </div>
                  <div className="space-y-2">
                    <StatusPill tone="neutral">{entry.actor}</StatusPill>
                    <p className="text-xs text-muted-foreground">
                      {formatTimestamp(entry.occurredAtUtc)}
                    </p>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </SectionCard>

        <SectionCard
          title="Import history"
          description="Completed and in-flight batches remain reviewable through persisted status records."
        >
          <div className="space-y-3">
            {imports.data?.map((batch) => (
              <article
                key={batch.id}
                className="rounded-xl border border-border bg-background/80 p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <strong className="text-sm text-foreground">
                      {batch.source.displayName || batch.strategyKey}
                    </strong>
                    <p className="mt-1 text-sm text-muted-foreground">
                      {formatTimestamp(batch.lastUpdatedAtUtc)}
                    </p>
                  </div>
                  <StatusPill
                    tone={
                      batch.status === 'Completed'
                        ? 'success'
                        : batch.status === 'Cancelled'
                          ? 'neutral'
                          : 'warning'
                    }
                  >
                    {formatImportBatchStatus(batch.status)}
                  </StatusPill>
                </div>
              </article>
            ))}
          </div>
        </SectionCard>
      </div>
    </div>
  );
}
