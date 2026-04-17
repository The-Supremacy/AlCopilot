import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import { toast } from 'sonner';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { Select } from '@/components/ui/select';
import { DiagnosticsSection } from '@/features/imports/DiagnosticsSection';
import {
  useApplyImportBatchMutation,
  useCancelImportBatchMutation,
  useImportBatch,
  useReviewImportBatchMutation,
} from '@/features/imports/useImportData';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';

export function ImportReviewPage() {
  const { batchId } = useParams({ from: '/imports/$batchId/review' });
  const batch = useImportBatch(batchId);
  const reviewMutation = useReviewImportBatchMutation();
  const applyMutation = useApplyImportBatchMutation();
  const cancelMutation = useCancelImportBatchMutation();
  const data = batch.data;
  const [aggregateFilter, setAggregateFilter] = useState('all');
  const [actionFilter, setActionFilter] = useState('all');
  const [reviewStateFilter, setReviewStateFilter] = useState('all');

  if (!data && !batch.isLoading) {
    return (
      <div className="space-y-4">
        <InlineMessage tone="danger" message="Import batch not found." />
        <Button asChild variant="ghost">
          <Link to="/imports">Back to imports</Link>
        </Button>
      </div>
    );
  }

  if (!data) {
    return <p className="text-sm text-muted-foreground">Loading review workspace...</p>;
  }

  const reviewIsStale = !data.reviewSummary || !data.reviewedAtUtc;
  const canApply = data.status === 'InProgress' && data.applyReadiness === 'Ready';
  const canCancel = data.status === 'InProgress';
  const isMutating =
    reviewMutation.isPending || applyMutation.isPending || cancelMutation.isPending;

  useEffect(() => {
    if (!data || !reviewIsStale || reviewMutation.isPending) {
      return;
    }

    if (data.status !== 'InProgress') {
      return;
    }

    reviewMutation.mutate(data.id);
  }, [data, reviewIsStale, reviewMutation]);

  const rows = useMemo(() => {
    const reviewRows = data.reviewRows ?? [];
    return reviewRows
      .filter((row) => aggregateFilter === 'all' || row.targetType === aggregateFilter)
      .filter((row) => actionFilter === 'all' || row.action === actionFilter)
      .filter((row) => {
        if (reviewStateFilter === 'all') return true;
        if (reviewStateFilter === 'updates') return row.requiresReview;
        if (reviewStateFilter === 'errors') return row.hasError;
        return !row.requiresReview && !row.hasError;
      });
  }, [actionFilter, aggregateFilter, reviewStateFilter, data.reviewRows]);

  const columns = useMemo<ColumnDef<(typeof rows)[number]>[]>(
    () => [
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
    ],
    [],
  );

  async function applyBatch() {
    try {
      await toast.promise(applyMutation.mutateAsync({ id: data.id }), {
        loading: 'Applying import changes...',
        success: (result) =>
          result.wasApplied
            ? 'Import applied successfully.'
            : result.applyReadiness === 'RequiresReview'
              ? 'Review is still required before apply.'
              : result.applyReadiness === 'BlockedByValidationErrors'
                ? 'Validation errors still block apply.'
                : 'Import was not applied.',
        error: getErrorMessage,
      });
    } catch {
      return;
    }
  }

  async function cancelBatch() {
    try {
      await toast.promise(cancelMutation.mutateAsync(data.id), {
        loading: 'Cancelling current import...',
        success: 'Import cancelled.',
        error: getErrorMessage,
      });
    } catch {
      return;
    }
  }

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Review</p>
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="font-display text-3xl font-semibold tracking-tight text-foreground">
            {data.source.displayName || data.strategyKey}
          </h1>
          <StatusPill
            tone={
              data.status === 'Completed'
                ? 'success'
                : data.status === 'Cancelled'
                  ? 'neutral'
                  : 'warning'
            }
          >
            {formatImportBatchStatus(data.status)}
          </StatusPill>
        </div>
        <p className="text-sm text-muted-foreground">
          Last updated {formatTimestamp(data.lastUpdatedAtUtc)}
        </p>
      </header>

      {reviewMutation.error ? (
        <InlineMessage tone="danger" message={reviewMutation.error.message} />
      ) : null}

      <SectionCard
        title="Review workspace"
        description="Review row-level changes and diagnostics in one place before applying the batch."
      >
        <div className="space-y-6">
          <div className="flex flex-wrap gap-3">
            <Button
              variant="outline"
              onClick={() => reviewMutation.mutate(data.id)}
              disabled={isMutating}
            >
              {reviewIsStale ? 'Generate review' : 'Refresh review'}
            </Button>
            <Button
              onClick={applyBatch}
              disabled={!canApply || isMutating}
              loading={applyMutation.isPending}
              loadingText="Applying..."
            >
              Apply
            </Button>
            <Button
              variant="outline"
              onClick={cancelBatch}
              disabled={!canCancel || isMutating}
              loading={cancelMutation.isPending}
              loadingText="Cancelling..."
            >
              Cancel
            </Button>
            <Button asChild variant="ghost">
              <Link to="/imports">Back to imports</Link>
            </Button>
          </div>

          {data.reviewSummary ? (
            <div className="rounded-2xl border border-border bg-background/80 p-4 text-sm text-muted-foreground">
              Review recorded: create {data.reviewSummary.createCount}, update{' '}
              {data.reviewSummary.updateCount}, skip {data.reviewSummary.skipCount}.
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
            <DataTable columns={columns} data={rows} searchPlaceholder="Search review rows" />
            <DiagnosticsSection diagnostics={data.diagnostics} applySummary={data.applySummary} />
          </div>
        </div>
      </SectionCard>
    </div>
  );
}

function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return 'Something went wrong.';
}
