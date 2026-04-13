import { Link } from '@tanstack/react-router';
import { StatusPill } from '@/components/StatusPill';
import { Button } from '@/components/ui/button';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';

type SelectedBatchSummaryProps = {
  batch: {
    id: string;
    strategyKey: string;
    status: string;
    lastUpdatedAtUtc: string;
    source: {
      displayName: string | null;
    };
  };
  reviewBatchId: string;
  onApply: () => void;
  onCancel: () => void;
  canApply: boolean;
  canCancel: boolean;
  applyHint?: string | null;
  isApplying: boolean;
  isCancelling: boolean;
};

export function SelectedBatchSummary({
  batch,
  reviewBatchId,
  onApply,
  onCancel,
  canApply,
  canCancel,
  applyHint,
  isApplying,
  isCancelling,
}: SelectedBatchSummaryProps) {
  const isBusy = isApplying || isCancelling;

  return (
    <>
      <div className="flex flex-col gap-4 rounded-2xl border border-border bg-background/80 p-4 md:flex-row md:items-start md:justify-between">
        <div className="space-y-1">
          <h3 className="text-lg font-semibold text-foreground">
            {batch.source.displayName || batch.strategyKey}
          </h3>
          <p className="text-sm text-muted-foreground">
            Last updated: {formatTimestamp(batch.lastUpdatedAtUtc)}
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

      <div className="flex flex-wrap gap-3">
        <Button asChild variant="outline" disabled={isBusy}>
          <Link to="/imports/$batchId/review" params={{ batchId: reviewBatchId }}>
            Review
          </Link>
        </Button>
        <Button
          onClick={onApply}
          disabled={!canApply || isBusy}
          loading={isApplying}
          loadingText="Applying..."
        >
          Apply
        </Button>
        <Button
          variant="outline"
          onClick={onCancel}
          disabled={!canCancel || isBusy}
          loading={isCancelling}
          loadingText="Cancelling..."
        >
          Cancel
        </Button>
      </div>
      {applyHint ? <p className="text-sm text-muted-foreground">{applyHint}</p> : null}
    </>
  );
}
