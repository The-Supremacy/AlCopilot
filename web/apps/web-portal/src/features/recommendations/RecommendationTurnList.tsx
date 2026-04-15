import type {
  RecommendationSessionDto,
  RecommendationTurnDto,
} from '@alcopilot/customer-api-client';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { formatRelativeDate } from '@/lib/format';

type RecommendationTurnListProps = {
  session: RecommendationSessionDto | null;
};

export function RecommendationTurnList({ session }: RecommendationTurnListProps) {
  if (!session || session.turns.length === 0) {
    return (
      <Card className="border-dashed bg-background/75">
        <CardContent className="space-y-3 py-8">
          <p className="font-display text-2xl text-foreground">Start the conversation</p>
          <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
            Ask for something refreshing, spirit-forward, low-prep, or based on what is already in
            your bar. Structured picks will appear here with clear make-now versus buy-next cues.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4" data-testid="recommendation-turn-list">
      {session.turns.map((turn) => (
        <RecommendationTurnCard key={turn.turnId} turn={turn} />
      ))}
    </div>
  );
}

function RecommendationTurnCard({ turn }: { turn: RecommendationTurnDto }) {
  const isAssistant = turn.role.toLowerCase() === 'assistant';

  return (
    <Card className={isAssistant ? 'bg-card/95' : 'bg-background/80'}>
      <CardContent className="space-y-4 py-6">
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant={isAssistant ? 'default' : 'secondary'}>
            {isAssistant ? 'Assistant' : 'You'}
          </Badge>
          <span className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
            {formatRelativeDate(turn.createdAtUtc)}
          </span>
        </div>

        <p className="whitespace-pre-wrap text-sm leading-7 text-foreground">{turn.content}</p>

        {turn.recommendationGroups.length > 0 ? (
          <div className="grid gap-4 lg:grid-cols-2">
            {turn.recommendationGroups.map((group) => (
              <section
                key={group.key}
                aria-label={group.label}
                className="rounded-2xl border border-border/70 bg-background/75 p-4"
              >
                <div className="mb-3 flex items-center gap-2">
                  <Badge variant={group.key === 'make-now' ? 'success' : 'warning'}>
                    {group.label}
                  </Badge>
                  <span className="text-xs text-muted-foreground">{group.items.length} drinks</span>
                </div>
                <div className="space-y-3">
                  {group.items.map((item) => (
                    <article
                      key={item.drinkId}
                      className="rounded-xl border border-border/60 bg-card/95 p-4"
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <h4 className="font-medium text-foreground">{item.drinkName}</h4>
                          {item.description ? (
                            <p className="mt-1 text-sm text-muted-foreground">{item.description}</p>
                          ) : null}
                        </div>
                        <Badge variant="neutral">Score {item.score}</Badge>
                      </div>

                      {item.matchedSignals.length > 0 ? (
                        <div className="mt-3 flex flex-wrap gap-2">
                          {item.matchedSignals.map((signal) => (
                            <Badge key={signal} variant="secondary">
                              {signal}
                            </Badge>
                          ))}
                        </div>
                      ) : null}

                      {item.missingIngredientNames.length > 0 ? (
                        <p className="mt-3 text-sm text-warning">
                          Buy next: {item.missingIngredientNames.join(', ')}
                        </p>
                      ) : (
                        <p className="mt-3 text-sm text-success">
                          Ready to make with your current bar.
                        </p>
                      )}
                    </article>
                  ))}
                </div>
              </section>
            ))}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
