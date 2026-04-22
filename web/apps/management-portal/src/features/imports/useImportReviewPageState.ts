import { useEffect, useMemo, useState } from 'react';
import { useParams } from '@tanstack/react-router';
import { toast } from 'sonner';
import type { ImportBatchDto } from '@alcopilot/management-api-client';
import {
  useApplyImportBatchMutation,
  useCancelImportBatchMutation,
  useImportBatch,
  useReviewImportBatchMutation,
} from '@/features/imports/useImportData';

type ReadyImportReviewPageState = {
  kind: 'ready';
  batch: ImportBatchDto;
  rows: ImportBatchDto['reviewRows'];
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
  applyBatch: () => Promise<void>;
  cancelBatch: () => Promise<void>;
};

type ImportReviewPageState =
  | { kind: 'loading' }
  | { kind: 'not-found' }
  | ReadyImportReviewPageState;

export function useImportReviewPageState(): ImportReviewPageState {
  const { batchId } = useParams({ from: '/imports/$batchId/review' });
  const batch = useImportBatch(batchId);
  const reviewMutation = useReviewImportBatchMutation();
  const applyMutation = useApplyImportBatchMutation();
  const cancelMutation = useCancelImportBatchMutation();
  const data = batch.data;
  const reviewRows = data?.reviewRows ?? [];
  const [aggregateFilter, setAggregateFilter] = useState('all');
  const [actionFilter, setActionFilter] = useState('all');
  const [reviewStateFilter, setReviewStateFilter] = useState('all');
  const reviewIsStale = !data?.reviewSummary || !data?.reviewedAtUtc;
  const canApply = data?.status === 'InProgress' && data.applyReadiness === 'Ready';
  const canCancel = data?.status === 'InProgress';
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
    return reviewRows
      .filter((row) => aggregateFilter === 'all' || row.targetType === aggregateFilter)
      .filter((row) => actionFilter === 'all' || row.action === actionFilter)
      .filter((row) => {
        if (reviewStateFilter === 'all') return true;
        if (reviewStateFilter === 'updates') return row.requiresReview;
        if (reviewStateFilter === 'errors') return row.hasError;
        return !row.requiresReview && !row.hasError;
      });
  }, [actionFilter, aggregateFilter, reviewRows, reviewStateFilter]);

  function refreshReview() {
    if (!data) {
      return;
    }

    reviewMutation.mutate(data.id);
  }

  async function applyBatch() {
    if (!data) {
      return;
    }

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
    if (!data) {
      return;
    }

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

  if (!data && !batch.isLoading) {
    return { kind: 'not-found' };
  }

  if (!data) {
    return { kind: 'loading' };
  }

  return {
    kind: 'ready',
    batch: data,
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
    isApplying: applyMutation.isPending,
    isCancelling: cancelMutation.isPending,
    reviewErrorMessage: reviewMutation.error ? getErrorMessage(reviewMutation.error) : null,
    refreshReview,
    applyBatch,
    cancelBatch,
  };
}

function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return 'Something went wrong.';
}
