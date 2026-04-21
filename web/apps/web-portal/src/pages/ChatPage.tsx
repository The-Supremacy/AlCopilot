import { useNavigate, useParams } from '@tanstack/react-router';
import { toast } from 'sonner';
import { InlineMessage } from '@/components/InlineMessage';
import {
  useSubmitRecommendationRequestMutation,
  useRecommendationSession,
} from '@/features/recommendations/hooks';
import { RecommendationComposer } from '@/features/recommendations/RecommendationComposer';
import { RecommendationTurnList } from '@/features/recommendations/RecommendationTurnList';

export function ChatPage() {
  const params = useParams({ strict: false });
  const navigate = useNavigate();
  const sessionId = typeof params.sessionId === 'string' ? params.sessionId : undefined;
  const sessionQuery = useRecommendationSession(sessionId);
  const submitMutation = useSubmitRecommendationRequestMutation();

  async function handleSubmit(message: string) {
    try {
      const session = await submitMutation.mutateAsync({
        sessionId: sessionId ?? null,
        message,
      });

      if (!sessionId) {
        await navigate({
          to: '/chat/$sessionId',
          params: { sessionId: session.sessionId },
        });
      }
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : 'Could not send recommendation request.',
      );
    }
  }

  return (
    <div className="mx-auto flex min-h-[calc(100vh-16rem)] w-full max-w-5xl flex-col">
      <div className="flex-1 space-y-5">
        {sessionQuery.isError ? (
          <InlineMessage
            tone="danger"
            message="The chat session could not be loaded. Try reopening it from the history rail."
          />
        ) : null}

        <RecommendationTurnList session={sessionQuery.data ?? null} />
      </div>

      <div className="sticky bottom-0 mt-6 border-t border-border/70 bg-[linear-gradient(180deg,transparent,hsl(var(--background))_18%)] pt-4">
        <div className="rounded-[28px] border border-border/70 bg-background/95 p-4 shadow-soft backdrop-blur md:p-5">
          <RecommendationComposer
            onSubmit={handleSubmit}
            isSubmitting={submitMutation.isPending || sessionQuery.isLoading}
          />
        </div>
      </div>
    </div>
  );
}
