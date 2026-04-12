import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from '@tanstack/react-router';
import type { ColumnDef } from '@tanstack/react-table';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { StatusPill } from '@/components/StatusPill';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { DiagnosticsSection } from '@/features/imports/DiagnosticsSection';
import {
  getBatchDecisions,
  useBatchDecisionMap,
  useUpsertImportDecision,
} from '@/features/imports/useImportDecisionStore';
import { formatTimestamp } from '@/lib/format';
import { formatImportBatchStatus } from '@/lib/importStatus';
import { useImportBatch, useReviewImportBatchMutation } from '@/lib/usePortalData';

export function ImportReviewPage() {
  const { batchId } = useParams({ from: '/imports/$batchId/review' });
  const batch = useImportBatch(batchId);
  const reviewMutation = useReviewImportBatchMutation();
  const decisionMap = useBatchDecisionMap(batchId);
  const upsertDecision = useUpsertImportDecision();
  const data = batch.data;
  const [aggregateFilter, setAggregateFilter] = useState('all');
  const [actionFilter, setActionFilter] = useState('all');
  const [conflictFilter, setConflictFilter] = useState('all');

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

  useEffect(() => {
    if (!data || !reviewIsStale || reviewMutation.isPending) {
      return;
    }

    if (data.status !== 'InProgress') {
      return;
    }

    reviewMutation.mutate(data.id);
  }, [data, reviewIsStale, reviewMutation]);

  const decisions = getBatchDecisions(data.id, data.reviewConflicts).map((decision) => {
    const key = `${decision.targetType}::${decision.targetKey}`.toLowerCase();
    return decisionMap[key] ?? decision;
  });
  const decisionByKey = new Map(
    decisions.map((decision) => [
      `${decision.targetType}::${decision.targetKey}`.toLowerCase(),
      decision,
    ]),
  );

  const rows = useMemo(() => {
    const reviewRows = data.reviewRows ?? [];
    return reviewRows
      .filter((row) => aggregateFilter === 'all' || row.targetType === aggregateFilter)
      .filter((row) => actionFilter === 'all' || row.action === actionFilter)
      .filter((row) => {
        if (conflictFilter === 'all') return true;
        if (conflictFilter === 'conflicts') return row.hasConflict;
        if (conflictFilter === 'errors') return row.hasError;
        return !row.hasConflict && !row.hasError;
      });
  }, [actionFilter, aggregateFilter, conflictFilter, data.reviewRows]);

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
          <span className="font-medium text-slate-950">{row.original.targetKey}</span>
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
          const key = `${row.original.targetType}::${row.original.targetKey}`.toLowerCase();
          const decision = decisionByKey.get(key);

          if (!row.original.hasConflict) {
            if (row.original.hasError) {
              return <span className="text-sm text-amber-700">See diagnostics</span>;
            }

            return <span className="text-sm text-muted-foreground">No decision required</span>;
          }

          return (
            <div className="flex min-w-[18rem] flex-col gap-2">
              <Select
                value={decision?.decision ?? 'approve-update'}
                onChange={(event) =>
                  upsertDecision(data.id, {
                    targetType: row.original.targetType,
                    targetKey: row.original.targetKey,
                    decision: event.target.value,
                    reason: decision?.reason ?? '',
                  })
                }
              >
                <option value="approve-update">Approve update</option>
                <option value="reject-update">Reject update</option>
              </Select>
              <Input
                value={decision?.reason ?? ''}
                onChange={(event) =>
                  upsertDecision(data.id, {
                    targetType: row.original.targetType,
                    targetKey: row.original.targetKey,
                    decision: decision?.decision ?? 'approve-update',
                    reason: event.target.value,
                  })
                }
                placeholder="Reason (optional)"
              />
            </div>
          );
        },
      },
    ],
    [data.id, decisionByKey, upsertDecision],
  );

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
        description="Review row-level changes, diagnostics, and conflict decisions in one place."
      >
        <div className="space-y-6">
          <div className="flex flex-wrap gap-3">
            <Button
              variant="outline"
              onClick={() => reviewMutation.mutate(data.id)}
              disabled={reviewMutation.isPending}
            >
              {reviewIsStale ? 'Generate review' : 'Refresh review'}
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
                value={conflictFilter}
                onChange={(event) => setConflictFilter(event.target.value)}
              >
                <option value="all">All rows</option>
                <option value="conflicts">Conflicts</option>
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
