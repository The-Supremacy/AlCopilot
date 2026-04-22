import { Link } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { Button } from '@/components/ui/button';
import { ImportReviewView } from '@/features/imports/ImportReviewView';
import { useImportReviewPageState } from '@/features/imports/useImportReviewPageState';

export function ImportReviewPage() {
  const state = useImportReviewPageState();

  if (state.kind === 'not-found') {
    return (
      <div className="space-y-4">
        <InlineMessage tone="danger" message="Import batch not found." />
        <Button asChild variant="ghost">
          <Link to="/imports">Back to imports</Link>
        </Button>
      </div>
    );
  }

  if (state.kind === 'loading') {
    return <p className="text-sm text-muted-foreground">Loading review workspace...</p>;
  }

  return <ImportReviewView {...state} />;
}
