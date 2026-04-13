import { StatusPill } from '@/components/StatusPill';

type Diagnostic = {
  code: string;
  message: string;
  severity: string;
};

type ApplySummary = {
  createdCount: number;
  updatedCount: number;
  skippedCount: number;
  rejectedCount: number;
};

type DiagnosticsSectionProps = {
  diagnostics: Diagnostic[];
  applySummary: ApplySummary | null | undefined;
};

export function DiagnosticsSection({ diagnostics, applySummary }: DiagnosticsSectionProps) {
  return (
    <div className="space-y-4">
      <div className="space-y-3">
        <h3 className="text-sm font-semibold text-foreground">Diagnostics</h3>
        {diagnostics.length > 0 ? (
          diagnostics.map((diagnostic) => (
            <div
              key={`${diagnostic.code}-${diagnostic.message}`}
              className="rounded-xl border border-border bg-background/80 p-4"
            >
              <div className="flex items-center justify-between gap-3">
                <strong className="text-sm text-foreground">{diagnostic.code}</strong>
                <StatusPill
                  tone={
                    diagnostic.severity === 'warning'
                      ? 'warning'
                      : diagnostic.severity === 'error'
                        ? 'danger'
                        : 'neutral'
                  }
                >
                  {diagnostic.severity}
                </StatusPill>
              </div>
              <p className="mt-2 text-sm text-muted-foreground">{diagnostic.message}</p>
            </div>
          ))
        ) : (
          <p className="text-sm text-muted-foreground">No diagnostics recorded yet.</p>
        )}
      </div>

      {applySummary ? (
        <div className="rounded-xl border border-border bg-background/80 p-4 text-sm">
          <h3 className="font-semibold text-foreground">Apply summary</h3>
          <p className="mt-2 text-muted-foreground">
            Created {applySummary.createdCount}, updated {applySummary.updatedCount}, skipped{' '}
            {applySummary.skippedCount}, rejected {applySummary.rejectedCount}.
          </p>
        </div>
      ) : null}
    </div>
  );
}
