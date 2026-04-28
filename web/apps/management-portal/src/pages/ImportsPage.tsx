import { ImportsView } from '@/features/imports/ImportsView';
import { useImportsPageState } from '@/features/imports/useImportsPageState';

export function ImportsPage() {
  const state = useImportsPageState();
  const currentBatch = state.currentBatch ?? null;

  const canApply =
    currentBatch?.status === 'InProgress' &&
    (currentBatch.applyReadiness === 'Ready' || currentBatch.applyReadiness === 'RequiresReview');
  const canCancel = currentBatch?.status === 'InProgress';

  const applyHint = !currentBatch
    ? null
    : currentBatch.status === 'Completed'
      ? 'This import has already been completed.'
      : currentBatch.status === 'Cancelled'
        ? 'This import has been cancelled.'
        : state.blockedByValidationErrors
          ? 'Validation errors block completion. Open Review to inspect diagnostics.'
          : state.requiresReviewBeforeApply
            ? 'Apply will refresh review first because this batch updates existing catalog data.'
            : null;

  return (
    <ImportsView
      strategyKey={state.strategyKey}
      currentBatch={currentBatch}
      importHistory={state.importHistory.data ?? []}
      activeErrorMessage={state.activeError?.message ?? null}
      canApply={Boolean(canApply)}
      canCancel={Boolean(canCancel)}
      applyHint={applyHint}
      isStartingImport={state.isStartingImport}
      isApplyingBatch={state.isApplyingBatch}
      isCancellingBatch={state.isCancellingBatch}
      startImport={state.startImport}
      applyBatch={state.applyBatch}
      cancelBatch={state.cancelBatch}
    />
  );
}
