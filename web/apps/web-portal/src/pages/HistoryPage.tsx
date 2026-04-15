import { Link } from '@tanstack/react-router';
import { InlineMessage } from '@/components/InlineMessage';
import { SectionCard } from '@/components/SectionCard';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { useRecommendationSessions } from '@/features/recommendations/hooks';
import { formatRelativeDate } from '@/lib/format';

export function HistoryPage() {
  const sessionsQuery = useRecommendationSessions();

  return (
    <SectionCard
      title="History"
      description="Revisit previous recommendation threads as lightweight entries instead of an operational table."
    >
      {sessionsQuery.isError ? (
        <InlineMessage
          tone="danger"
          message="Recommendation history could not be loaded. Try again from the session rail."
        />
      ) : null}

      <div className="space-y-4">
        {(sessionsQuery.data ?? []).map((session) => (
          <article
            key={session.sessionId}
            className="rounded-2xl border border-border/70 bg-background/80 p-5"
          >
            <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
              <div className="space-y-2">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="font-display text-xl text-foreground">{session.title}</h3>
                  <Badge variant="secondary">{formatRelativeDate(session.updatedAtUtc)}</Badge>
                </div>
                <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
                  {session.lastAssistantMessage || 'No assistant summary available yet.'}
                </p>
              </div>
              <Button asChild>
                <Link to="/chat/$sessionId" params={{ sessionId: session.sessionId }}>
                  Revisit session
                </Link>
              </Button>
            </div>
          </article>
        ))}
      </div>
    </SectionCard>
  );
}
