import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { BatchHistorySection } from '@/features/imports/BatchHistorySection';
import { DiagnosticsSection } from '@/features/imports/DiagnosticsSection';
import { ImportPresetSection } from '@/features/imports/ImportPresetSection';
import { SelectedBatchSummary } from '@/features/imports/SelectedBatchSummary';
import { useImportsPageState } from '@/features/imports/useImportsPageState';

export function ImportsPage() {
  const state = useImportsPageState();
  const currentBatch = state.currentBatch;

  const reviewConflicts = currentBatch?.reviewConflicts ?? [];
  const hasConflicts = reviewConflicts.length > 0;
  const hasUnresolvedConflicts = hasConflicts && !state.hasStoredDecisionForAllConflicts;

  const hasValidationErrors =
    currentBatch?.diagnostics.some((diagnostic) => diagnostic.severity === 'error') ?? false;
  const canApply =
    currentBatch?.status === 'InProgress' && !hasValidationErrors && !hasUnresolvedConflicts;
  const canCancel = currentBatch?.status === 'InProgress';

  const applyHint = !currentBatch
    ? null
    : currentBatch.status === 'Completed'
      ? 'This import has already been completed.'
      : currentBatch.status === 'Cancelled'
        ? 'This import has been cancelled.'
        : hasValidationErrors
          ? 'Validation errors block completion. Open Review to inspect diagnostics.'
          : hasUnresolvedConflicts
            ? 'Conflicts are present. Open Review to record decisions before applying.'
            : null;

  return (
    <div className="space-y-6">
      <header className="space-y-2">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">Imports</p>
        <h1 className="font-display text-3xl font-semibold tracking-tight text-foreground">
          Review current imports, then apply when ready
        </h1>
      </header>

      {state.activeError ? (
        <InlineMessage tone="danger" message={state.activeError.message} />
      ) : null}

      {currentBatch ? (
        <SectionCard
          title="Current import"
          description="Start validates immediately. Review is optional and completion stays blocked until errors and conflicts are cleared."
        >
          <div className="space-y-6">
            <SelectedBatchSummary
              batch={currentBatch}
              reviewBatchId={currentBatch.id}
              onApply={state.applyBatch}
              onCancel={state.cancelBatch}
              canApply={Boolean(canApply)}
              canCancel={Boolean(canCancel)}
              applyHint={applyHint}
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
              <DiagnosticsSection
                diagnostics={currentBatch.diagnostics}
                applySummary={currentBatch.applySummary}
              />
              <div className="rounded-2xl border border-border bg-background/80 p-4 text-sm">
                <h3 className="font-display font-semibold text-foreground">Current report</h3>
                {currentBatch.reviewSummary ? (
                  <p className="mt-2 text-muted-foreground">
                    Review recorded: create {currentBatch.reviewSummary.createCount}, update{' '}
                    {currentBatch.reviewSummary.updateCount}, skip{' '}
                    {currentBatch.reviewSummary.skipCount}.
                  </p>
                ) : (
                  <p className="mt-2 text-muted-foreground">
                    Start import prepares the current review snapshot immediately. You can apply
                    directly when there are no errors or unresolved conflicts, or open Review to
                    inspect row-level changes first.
                  </p>
                )}
                <p className="mt-3 text-muted-foreground">
                  Conflict rows: {currentBatch.reviewConflicts.length}. Review decisions are kept
                  when you move between the current import page and Review.
                </p>
              </div>
            </div>
          </div>
        </SectionCard>
      ) : (
        <ImportPresetSection strategyKey={state.strategyKey} onSubmit={state.startImport} />
      )}

      <BatchHistorySection batches={state.importHistory.data ?? []} />
    </div>
  );
}
