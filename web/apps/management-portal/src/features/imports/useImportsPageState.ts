import { useMemo, useState } from 'react';
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
    await startImportMutation.mutateAsync({
      strategyKey,
      payload: '',
      source: {
        sourceReference: null,
        displayName: null,
        contentType: 'application/json',
        metadata: {},
      },
    });
  }

  function applyBatch() {
    if (!currentBatch) {
      return;
    }

    const decisions = getStoredBatchDecisions(currentBatch.id);

    applyMutation.mutate(
      {
        id: currentBatch.id,
        input: { overrideDuplicateFingerprint: false, decisions },
      },
      {
        onSuccess: () => clearBatchDecisions(currentBatch.id),
      },
    );
  }

  function cancelBatch() {
    if (!currentBatch) {
      return;
    }

    cancelMutation.mutate(currentBatch.id, {
      onSuccess: () => clearBatchDecisions(currentBatch.id),
    });
  }

  return {
    strategyKey,
    decisionState,
    importHistory,
    currentBatch,
    activeError,
    hasStoredDecisionForAllConflicts,
    startImport,
    applyBatch,
    cancelBatch,
  };
}
