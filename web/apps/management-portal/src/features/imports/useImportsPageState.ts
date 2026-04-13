import { useMemo, useState } from 'react';
import { toast } from 'sonner';
import {
  useApplyImportBatchMutation,
  useCancelImportBatchMutation,
  useImportBatch,
  useImportHistory,
  useStartImportMutation,
} from '@/lib/usePortalData';
import {
  getStoredBatchDecisions,
  useBatchDecisionMap,
  useClearImportDecisionBatch,
} from '@/features/imports/useImportDecisionStore';

export function useImportsPageState() {
  const [strategyKey] = useState('iba-cocktails-snapshot');
  const importHistory = useImportHistory();
  const activeBatchId = useMemo(() => {
    const batches = importHistory.data ?? [];
    return batches.find((batch) => batch.status === 'InProgress')?.id ?? null;
  }, [importHistory.data]);
  const selectedBatch = useImportBatch(activeBatchId);
  const startImportMutation = useStartImportMutation();
  const applyMutation = useApplyImportBatchMutation();
  const cancelMutation = useCancelImportBatchMutation();
  const decisionState = useBatchDecisionMap(activeBatchId);
  const clearBatchDecisions = useClearImportDecisionBatch();

  const currentBatch = selectedBatch.data;
  const activeError = startImportMutation.error ?? applyMutation.error ?? cancelMutation.error;
  const hasStoredDecisionForAllConflicts = currentBatch
    ? currentBatch.reviewConflicts.every((conflict) => {
        const key = `${conflict.targetType.trim().toLowerCase()}::${conflict.targetKey.trim().toLowerCase()}`;
        return Boolean(decisionState[key]);
      })
    : false;

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

    const decisions = getStoredBatchDecisions(currentBatch.id);
    const applyPromise = applyMutation
      .mutateAsync({
        id: currentBatch.id,
        input: { overrideDuplicateFingerprint: false, decisions },
      })
      .then((result) => {
        clearBatchDecisions(currentBatch.id);
        return result;
      });

    await toast.promise(applyPromise, {
      loading: 'Applying import changes...',
      success: 'Import applied successfully.',
      error: getErrorMessage,
    });
  }

  async function cancelBatch() {
    if (!currentBatch) {
      return;
    }

    const cancelPromise = cancelMutation.mutateAsync(currentBatch.id).then((result) => {
      clearBatchDecisions(currentBatch.id);
      return result;
    });

    await toast.promise(cancelPromise, {
      loading: 'Cancelling current import...',
      success: 'Import cancelled.',
      error: getErrorMessage,
    });
  }

  return {
    strategyKey,
    decisionState,
    importHistory,
    currentBatch,
    activeError,
    hasStoredDecisionForAllConflicts,
    isStartingImport: startImportMutation.isPending,
    isApplyingBatch: applyMutation.isPending,
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
