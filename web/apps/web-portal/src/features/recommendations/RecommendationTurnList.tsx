import { Fragment, type ReactNode } from 'react';
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
            your bar. Structured picks will appear here with clear available-now versus restock
            cues.
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
    <div className={isAssistant ? 'flex justify-start' : 'flex justify-end'}>
      <Card
        className={
          isAssistant
            ? 'w-full max-w-3xl border-border/70 bg-card/95'
            : 'w-full max-w-2xl border-primary/25 bg-primary/8'
        }
      >
        <CardContent className="space-y-4 py-5">
          <div
            className={
              isAssistant
                ? 'flex flex-wrap items-center gap-2'
                : 'flex flex-wrap items-center justify-end gap-2 text-right'
            }
          >
            <span className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
              {formatRelativeDate(turn.createdAtUtc)}
            </span>
            <Badge variant={isAssistant ? 'default' : 'secondary'}>
              {isAssistant ? 'Assistant' : 'You'}
            </Badge>
          </div>

          {isAssistant ? (
            <div className="space-y-3 text-sm leading-7 text-foreground">
              {renderAssistantContent(turn.content)}
            </div>
          ) : (
            <p className="whitespace-pre-wrap text-right text-sm leading-7 text-foreground">
              {turn.content}
            </p>
          )}

          {turn.recommendationGroups.length > 0 ? (
            <div className="space-y-4 rounded-2xl border border-border/70 bg-background/70 p-4">
              {turn.recommendationGroups.map((group) => (
                <section key={group.key} aria-label={group.label} className="space-y-3">
                  <div className="flex items-center gap-2">
                    <Badge variant={group.key === 'make-now' ? 'success' : 'warning'}>
                      {group.label}
                    </Badge>
                    <span className="text-xs text-muted-foreground">
                      {group.items.length} drinks
                    </span>
                  </div>
                  <ul className="space-y-3">
                    {group.items.map((item) => (
                      <li
                        key={item.drinkId}
                        className="rounded-xl border border-border/60 bg-card/95 px-4 py-3"
                      >
                        <div className="flex flex-wrap items-center gap-2">
                          <h4 className="font-medium text-foreground">{item.drinkName}</h4>
                          <Badge variant="neutral">Score {item.score}</Badge>
                        </div>
                        {item.description ? (
                          <p className="mt-1 text-sm text-muted-foreground">{item.description}</p>
                        ) : null}
                        <ul className="mt-3 space-y-1 text-sm leading-6 text-foreground">
                          {item.recipeEntries && item.recipeEntries.length > 0 ? (
                            <li>
                              <span className="text-muted-foreground">Recipe:</span>{' '}
                              {item.recipeEntries.map((entry) => (
                                <Fragment key={`${item.drinkId}-${entry.ingredientName}`}>
                                  <span
                                    className={
                                      entry.isOwned
                                        ? 'font-medium text-foreground'
                                        : 'text-foreground'
                                    }
                                  >
                                    {entry.ingredientName} ({entry.quantity})
                                  </span>
                                  {!entry.isOwned ? (
                                    <span className="text-muted-foreground"> need to restock</span>
                                  ) : null}
                                  {entry !== item.recipeEntries[item.recipeEntries.length - 1]
                                    ? ', '
                                    : null}
                                </Fragment>
                              ))}
                            </li>
                          ) : null}
                          {item.matchedSignals.length > 0 ? (
                            <li>Matches: {item.matchedSignals.join(', ')}</li>
                          ) : null}
                          {item.missingIngredientNames.length > 0 ? (
                            <li>Consider for restock: {item.missingIngredientNames.join(', ')}</li>
                          ) : (
                            <li>Available now: everything is already in your bar.</li>
                          )}
                        </ul>
                      </li>
                    ))}
                  </ul>
                </section>
              ))}
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}

function renderAssistantContent(content: string): ReactNode[] {
  const lines = content.split(/\r?\n/);
  const nodes: ReactNode[] = [];
  let paragraphLines: string[] = [];
  let bulletLines: string[] = [];

  function flushParagraph() {
    if (paragraphLines.length === 0) {
      return;
    }

    nodes.push(
      <p key={`paragraph-${nodes.length}`} className="whitespace-pre-wrap">
        {renderInlineFormatting(paragraphLines.join(' '))}
      </p>,
    );
    paragraphLines = [];
  }

  function flushBullets() {
    if (bulletLines.length === 0) {
      return;
    }

    nodes.push(
      <ul key={`bullets-${nodes.length}`} className="list-disc space-y-1 pl-5 marker:text-primary">
        {bulletLines.map((line, index) => (
          <li key={`bullet-${index}`}>{renderInlineFormatting(line)}</li>
        ))}
      </ul>,
    );
    bulletLines = [];
  }

  for (const line of lines) {
    const trimmed = line.trim();

    if (trimmed.length === 0) {
      flushParagraph();
      flushBullets();
      continue;
    }

    if (trimmed.startsWith('* ')) {
      flushParagraph();
      bulletLines.push(trimmed.slice(2).trim());
      continue;
    }

    flushBullets();
    paragraphLines.push(trimmed);
  }

  flushParagraph();
  flushBullets();

  return nodes;
}

function renderInlineFormatting(text: string): ReactNode[] {
  const segments = text.split(/(\*\*.+?\*\*)/g).filter(Boolean);

  return segments.map((segment, index) => {
    const isHighlighted = segment.startsWith('**') && segment.endsWith('**') && segment.length > 4;
    if (!isHighlighted) {
      return <Fragment key={index}>{segment}</Fragment>;
    }

    return (
      <mark
        key={index}
        className="rounded-md bg-primary/15 px-1.5 py-0.5 font-medium text-foreground"
      >
        {segment.slice(2, -2)}
      </mark>
    );
  });
}
