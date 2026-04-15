import type { ImportDecisionInput } from '@alcopilot/management-api-client';
import { create } from 'zustand';

type ImportDecisionStoreState = {
  decisionsByBatch: Record<string, Record<string, ImportDecisionInput>>;
  upsertDecision: (batchId: string, decision: ImportDecisionInput) => void;
  clearBatch: (batchId: string) => void;
};

const emptyDecisionMap: Record<string, ImportDecisionInput> = {};

function buildDecisionKey(targetType: string, targetKey: string) {
  return `${targetType.trim().toLowerCase()}::${targetKey.trim().toLowerCase()}`;
}

const useImportDecisionStore = create<ImportDecisionStoreState>((set) => ({
  decisionsByBatch: {},
  upsertDecision: (batchId, decision) =>
    set((state) => ({
      decisionsByBatch: {
        ...state.decisionsByBatch,
        [batchId]: {
          ...(state.decisionsByBatch[batchId] ?? {}),
          [buildDecisionKey(decision.targetType, decision.targetKey)]: decision,
        },
      },
    })),
  clearBatch: (batchId) =>
    set((state) => {
      const next = { ...state.decisionsByBatch };
      delete next[batchId];
      return { decisionsByBatch: next };
    }),
}));

export function useBatchDecisionMap(batchId: string | null) {
  return useImportDecisionStore((state) =>
    batchId ? (state.decisionsByBatch[batchId] ?? emptyDecisionMap) : emptyDecisionMap,
  );
}

export function useUpsertImportDecision() {
  return useImportDecisionStore((state) => state.upsertDecision);
}

export function useClearImportDecisionBatch() {
  return useImportDecisionStore((state) => state.clearBatch);
}

export function getBatchDecisions(
  batchId: string,
  conflicts: Array<{ targetType: string; targetKey: string }>,
): ImportDecisionInput[] {
  const stored = useImportDecisionStore.getState().decisionsByBatch[batchId] ?? {};

  return conflicts.map((conflict) => {
    const key = buildDecisionKey(conflict.targetType, conflict.targetKey);
    return (
      stored[key] ?? {
        targetType: conflict.targetType,
        targetKey: conflict.targetKey,
        decision: 'approve-update',
        reason: '',
      }
    );
  });
}

export function getStoredBatchDecisions(batchId: string): ImportDecisionInput[] {
  return Object.values(
    useImportDecisionStore.getState().decisionsByBatch[batchId] ?? emptyDecisionMap,
  );
}
