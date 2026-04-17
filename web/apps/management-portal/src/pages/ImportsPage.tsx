import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { BatchHistorySection } from '@/features/imports/BatchHistorySection';
import { DiagnosticsSection } from '@/features/imports/DiagnosticsSection';
import { ImportPresetSection } from '@/features/imports/ImportPresetSection';
import { SelectedBatchSummary } from '@/features/imports/SelectedBatchSummary';
import { useImportsPageState } from '@/features/imports/useImportsPageState';
import { LoaderCircle } from 'lucide-react';

export function ImportsPage() {
  const state = useImportsPageState();
  const currentBatch = state.currentBatch;

  const canApply = currentBatch?.status === 'InProgress' && currentBatch.applyReadiness === 'Ready';
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
            ? 'This batch updates existing catalog data. Open Review before applying.'
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
          description="Start validates immediately. Review stays focused on inspection, and apply remains blocked until validation errors are cleared and required review is completed."
        >
          <div className="relative space-y-6">
            {state.isApplyingBatch ? (
              <div className="absolute inset-0 z-10 flex items-center justify-center rounded-2xl border border-border/70 bg-background/75 backdrop-blur-sm">
                <div className="flex items-center gap-3 rounded-2xl border border-border bg-card px-4 py-3 shadow-soft">
                  <LoaderCircle className="h-4 w-4 animate-spin text-primary" />
                  <div>
                    <p className="text-sm font-medium text-foreground">Applying import changes</p>
                    <p className="text-xs text-muted-foreground">
                      The workspace is temporarily locked while the apply step completes.
                    </p>
                  </div>
                </div>
              </div>
            ) : null}
            <SelectedBatchSummary
              batch={currentBatch}
              reviewBatchId={currentBatch.id}
              onApply={state.applyBatch}
              onCancel={state.cancelBatch}
              canApply={Boolean(canApply)}
              canCancel={Boolean(canCancel)}
              applyHint={applyHint}
              isApplying={state.isApplyingBatch}
              isCancelling={state.isCancellingBatch}
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
                    directly when there are no blocking diagnostics or review-required updates, or
                    open Review to inspect row-level changes first.
                  </p>
                )}
                <p className="mt-3 text-muted-foreground">
                  Review-required rows:{' '}
                  {currentBatch.reviewRows.filter((row) => row.requiresReview).length}. Update rows
                  remain visible on the Review page and no longer require row-level decisions.
                </p>
              </div>
            </div>
          </div>
        </SectionCard>
      ) : (
        <ImportPresetSection
          strategyKey={state.strategyKey}
          onSubmit={state.startImport}
          isSubmitting={state.isStartingImport}
        />
      )}

      <BatchHistorySection batches={state.importHistory.data ?? []} />
    </div>
  );
}
