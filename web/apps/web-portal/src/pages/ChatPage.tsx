import { useNavigate, useParams } from '@tanstack/react-router';
import { toast } from 'sonner';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
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
    <div className="grid gap-5 xl:grid-cols-[minmax(0,1.4fr)_360px]">
      <div className="space-y-5">
        {sessionQuery.isError ? (
          <InlineMessage
            tone="danger"
            message="The chat session could not be loaded. Try reopening it from the history rail."
          />
        ) : null}

        <RecommendationTurnList session={sessionQuery.data ?? null} />
      </div>

      <div className="space-y-5">
        <SectionCard
          title={sessionId ? 'Continue the session' : 'Start a recommendation chat'}
          description="Describe the mood, spirits, ingredients, or effort level you want. The assistant will answer with prose plus structured groups."
        >
          <RecommendationComposer
            onSubmit={handleSubmit}
            isSubmitting={submitMutation.isPending || sessionQuery.isLoading}
          />
        </SectionCard>

        <SectionCard
          title="How this stays grounded"
          description="The first customer slice keeps filtering and grouping deterministic before the model adds explanation."
        >
          <div className="space-y-3 text-sm leading-6 text-muted-foreground">
            <p>Prohibited ingredients are removed before ranking.</p>
            <p>`Make now` and `buy next` stay visually explicit and do not rely on prose alone.</p>
            <p>
              Profile changes in `My Bar` and `Preferences` shape future replies without extra token
              handling in the browser.
            </p>
          </div>
        </SectionCard>
      </div>
    </div>
  );
}
