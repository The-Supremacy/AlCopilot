import { useMemo, useState } from 'react';
import { toast } from 'sonner';
import {
  useApplyImportBatchMutation,
  useCancelImportBatchMutation,
  useImportBatch,
  useImportHistory,
  useReviewImportBatchMutation,
  useStartImportMutation,
} from '@/features/imports/useImportData';

export function useImportsPageState() {
  const [strategyKey] = useState('iba-cocktails-snapshot');
  const importHistory = useImportHistory();
  const latestBatch = useMemo(() => (importHistory.data ?? [])[0] ?? null, [importHistory.data]);
  const activeBatchId = latestBatch?.status === 'InProgress' ? latestBatch.id : null;
  const selectedBatch = useImportBatch(activeBatchId);
  const startImportMutation = useStartImportMutation();
  const reviewMutation = useReviewImportBatchMutation();
  const applyMutation = useApplyImportBatchMutation();
  const cancelMutation = useCancelImportBatchMutation();

  const currentBatch = selectedBatch.data;
  const activeError =
    startImportMutation.error ??
    reviewMutation.error ??
    applyMutation.error ??
    cancelMutation.error;
  const requiresReviewBeforeApply = currentBatch?.applyReadiness === 'RequiresReview';
  const blockedByValidationErrors = currentBatch?.applyReadiness === 'BlockedByValidationErrors';

  async function startImport() {
    await toast.promise(
      startImportMutation.mutateAsync({
        strategyKey,
        payload: '',
        source: {
          sourceReference: null,
          displayName: null,
          contentType: 'application/json',
          metadata: {},
        },
      }),
      {
        loading: 'Starting import preset...',
        success: 'Import preset started.',
        error: getErrorMessage,
      },
    );
  }

  async function applyBatch() {
    if (!currentBatch) {
      return;
    }

    try {
      await toast.promise(
        (async () => {
          const reviewedBatch =
            currentBatch.applyReadiness === 'RequiresReview'
              ? await reviewMutation.mutateAsync(currentBatch.id)
              : currentBatch;

          if (reviewedBatch.applyReadiness !== 'Ready') {
            return {
              batch: reviewedBatch,
              applyReadiness: reviewedBatch.applyReadiness,
              wasApplied: false,
            };
          }

          return await applyMutation.mutateAsync({
            id: reviewedBatch.id,
          });
        })(),
        {
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
        },
      );
    } catch {
      return;
    }
  }

  async function cancelBatch() {
    if (!currentBatch) {
      return;
    }

    const cancelPromise = cancelMutation.mutateAsync(currentBatch.id);

    await toast.promise(cancelPromise, {
      loading: 'Cancelling current import...',
      success: 'Import cancelled.',
      error: getErrorMessage,
    });
  }

  return {
    strategyKey,
    importHistory,
    currentBatch,
    activeError,
    requiresReviewBeforeApply,
    blockedByValidationErrors,
    isStartingImport: startImportMutation.isPending,
    isApplyingBatch: reviewMutation.isPending || applyMutation.isPending,
    isCancellingBatch: cancelMutation.isPending,
    startImport,
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
