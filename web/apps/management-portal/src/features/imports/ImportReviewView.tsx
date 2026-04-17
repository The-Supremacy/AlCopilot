import { Link } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import type { ImportBatchDto, ImportReviewRowDto } from '@alcopilot/management-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { Select } from '@/components/ui/select';
import { DiagnosticsSection } from '@/features/imports/DiagnosticsSection';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';

type ImportReviewViewProps = {
  batch: ImportBatchDto;
  rows: ImportReviewRowDto[];
  aggregateFilter: string;
  actionFilter: string;
  reviewStateFilter: string;
  setAggregateFilter: (value: string) => void;
  setActionFilter: (value: string) => void;
  setReviewStateFilter: (value: string) => void;
  reviewIsStale: boolean;
  canApply: boolean;
  canCancel: boolean;
  isMutating: boolean;
  isApplying: boolean;
  isCancelling: boolean;
  reviewErrorMessage: string | null;
  refreshReview: () => void;
  applyBatch: () => void;
  cancelBatch: () => void;
};

const reviewColumns: ColumnDef<ImportReviewRowDto>[] = [
  {
    accessorKey: 'targetType',
    header: 'Aggregate',
    meta: { label: 'Aggregate' },
    cell: ({ row }) => row.original.targetType,
  },
  {
    accessorKey: 'targetKey',
    header: 'Target',
    meta: { label: 'Target' },
    cell: ({ row }) => (
      <span className="font-medium text-foreground">{row.original.targetKey}</span>
    ),
  },
  {
    accessorKey: 'action',
    header: 'Action',
    meta: { label: 'Action' },
    cell: ({ row }) => (
      <StatusPill
        tone={
          row.original.action === 'create'
            ? 'success'
            : row.original.action === 'update'
              ? 'warning'
              : 'neutral'
        }
      >
        {row.original.action}
      </StatusPill>
    ),
  },
  {
    accessorKey: 'changeSummary',
    header: 'Summary',
    meta: { label: 'Summary' },
  },
  {
    id: 'review',
    header: 'Review',
    meta: { label: 'Review' },
    enableSorting: false,
    cell: ({ row }) => {
      if (row.original.hasError) {
        return <span className="text-sm text-warning">See diagnostics</span>;
      }

      if (row.original.requiresReview) {
        return <span className="text-sm text-warning">Review before apply</span>;
      }

      return <span className="text-sm text-muted-foreground">No review required</span>;
    },
  },
];

export function ImportReviewView({
  batch,
  rows,
  aggregateFilter,
  actionFilter,
  reviewStateFilter,
  setAggregateFilter,
  setActionFilter,
  setReviewStateFilter,
  reviewIsStale,
  canApply,
  canCancel,
  isMutating,
  isApplying,
  isCancelling,
  reviewErrorMessage,
  refreshReview,
  applyBatch,
  cancelBatch,
}: ImportReviewViewProps) {
  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Review</p>
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="font-display text-3xl font-semibold tracking-tight text-foreground">
            {batch.source.displayName || batch.strategyKey}
          </h1>
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
        <p className="text-sm text-muted-foreground">
          Last updated {formatTimestamp(batch.lastUpdatedAtUtc)}
        </p>
      </header>

      {reviewErrorMessage ? <InlineMessage tone="danger" message={reviewErrorMessage} /> : null}

      <SectionCard
        title="Review workspace"
        description="Review row-level changes and diagnostics in one place before applying the batch."
      >
        <div className="space-y-6">
          <div className="flex flex-wrap gap-3">
            <Button variant="outline" onClick={refreshReview} disabled={isMutating}>
              {reviewIsStale ? 'Generate review' : 'Refresh review'}
            </Button>
            <Button
              onClick={applyBatch}
              disabled={!canApply || isMutating}
              loading={isApplying}
              loadingText="Applying..."
            >
              Apply
            </Button>
            <Button
              variant="outline"
              onClick={cancelBatch}
              disabled={!canCancel || isMutating}
              loading={isCancelling}
              loadingText="Cancelling..."
            >
              Cancel
            </Button>
            <Button asChild variant="ghost">
              <Link to="/imports">Back to imports</Link>
            </Button>
          </div>

          {batch.reviewSummary ? (
            <div className="rounded-2xl border border-border bg-background/80 p-4 text-sm text-muted-foreground">
              Review recorded: create {batch.reviewSummary.createCount}, update{' '}
              {batch.reviewSummary.updateCount}, skip {batch.reviewSummary.skipCount}.
            </div>
          ) : (
            <div className="rounded-2xl border border-border bg-background/80 p-4 text-sm text-muted-foreground">
              No review recorded yet. Generate review to inspect planned changes. Validation
              diagnostics already appear below.
            </div>
          )}

          <div className="grid gap-3 md:grid-cols-3">
            <label className="space-y-2 text-sm font-medium">
              <span>Aggregate</span>
              <Select
                value={aggregateFilter}
                onChange={(event) => setAggregateFilter(event.target.value)}
              >
                <option value="all">All aggregates</option>
                <option value="drink">Drinks</option>
                <option value="ingredient">Ingredients</option>
                <option value="tag">Tags</option>
              </Select>
            </label>
            <label className="space-y-2 text-sm font-medium">
              <span>Action</span>
              <Select
                value={actionFilter}
                onChange={(event) => setActionFilter(event.target.value)}
              >
                <option value="all">All actions</option>
                <option value="create">Create</option>
                <option value="update">Update</option>
                <option value="skip">Skip</option>
              </Select>
            </label>
            <label className="space-y-2 text-sm font-medium">
              <span>Review state</span>
              <Select
                value={reviewStateFilter}
                onChange={(event) => setReviewStateFilter(event.target.value)}
              >
                <option value="all">All rows</option>
                <option value="updates">Requires review</option>
                <option value="errors">Errors</option>
                <option value="clean">Clean rows</option>
              </Select>
            </label>
          </div>

          <div className="grid gap-6 xl:grid-cols-[1.35fr_0.65fr]">
            <DataTable columns={reviewColumns} data={rows} searchPlaceholder="Search review rows" />
            <DiagnosticsSection diagnostics={batch.diagnostics} applySummary={batch.applySummary} />
          </div>
        </div>
      </SectionCard>
    </div>
  );
}
