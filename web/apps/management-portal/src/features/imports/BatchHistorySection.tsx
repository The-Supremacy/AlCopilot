import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';
import { Link } from '@tanstack/react-router';

type BatchHistoryItem = {
  id: string;
  strategyKey: string;
  status: string;
  lastUpdatedAtUtc: string;
  source: {
    displayName: string | null;
  };
};

type BatchHistorySectionProps = {
  batches: BatchHistoryItem[];
};

export function BatchHistorySection({ batches }: BatchHistorySectionProps) {
  return (
    <SectionCard
      title="History"
      description="Review previous imports and reopen Review when deeper inspection is needed."
    >
      <div className="space-y-3">
        {batches.map((batch) => (
          <div
            key={batch.id}
            className="w-full rounded-xl border border-border bg-background/80 px-4 py-4 text-left transition-colors hover:bg-accent/50"
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
              <div className="flex items-center gap-3">
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
                <Link
                  to="/imports/$batchId/review"
                  params={{ batchId: batch.id }}
                  className="text-xs font-semibold uppercase tracking-wide text-primary"
                >
                  Review
                </Link>
              </div>
            </div>
          </div>
        ))}
        {batches.length === 0 ? (
          <p className="text-sm text-muted-foreground">No import batches recorded yet.</p>
        ) : null}
      </div>
    </SectionCard>
  );
}
