import { Fragment, useState, type ReactNode } from 'react';
import type {
  RecommendationSessionDto,
  RecommendationTurnDto,
} from '@alcopilot/customer-api-client';
import { ChevronDown, ThumbsDown, ThumbsUp } from 'lucide-react';
import { Badge, type BadgeProps } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { cn } from '@/lib/utils';
import { formatRelativeDate } from '@/lib/format';

type RecommendationTurnListProps = {
  session: RecommendationSessionDto | null;
  onFeedback?: (turnId: string, rating: 'positive' | 'negative') => Promise<void>;
  isFeedbackPending?: boolean;
};

export function RecommendationTurnList({
  session,
  onFeedback,
  isFeedbackPending = false,
}: RecommendationTurnListProps) {
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
        <RecommendationTurnCard
          key={turn.turnId}
          turn={turn}
          onFeedback={onFeedback}
          isFeedbackPending={isFeedbackPending}
        />
      ))}
    </div>
  );
}

function RecommendationTurnCard({
  turn,
  onFeedback,
  isFeedbackPending,
}: {
  turn: RecommendationTurnDto;
  onFeedback?: (turnId: string, rating: 'positive' | 'negative') => Promise<void>;
  isFeedbackPending: boolean;
}) {
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

          <RecommendationResultDisclosure groups={turn.recommendationGroups} />

          {isAssistant && onFeedback ? (
            <div className="flex items-center justify-end gap-2 border-t border-border/60 pt-3">
              <button
                type="button"
                className={cn(
                  'inline-flex h-8 w-8 shrink-0 cursor-pointer items-center justify-center rounded-md p-0 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50',
                  turn.feedback?.rating === 'positive' && 'bg-success-muted text-success',
                )}
                aria-label="Mark response helpful"
                aria-pressed={turn.feedback?.rating === 'positive'}
                title="Mark response helpful"
                disabled={isFeedbackPending}
                onClick={() => onFeedback(turn.turnId, 'positive')}
              >
                <ThumbsUp className="h-4 w-4" />
              </button>
              <button
                type="button"
                className={cn(
                  'inline-flex h-8 w-8 shrink-0 cursor-pointer items-center justify-center rounded-md p-0 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50',
                  turn.feedback?.rating === 'negative' && 'bg-destructive-muted text-destructive',
                )}
                aria-label="Mark response unhelpful"
                aria-pressed={turn.feedback?.rating === 'negative'}
                title="Mark response unhelpful"
                disabled={isFeedbackPending}
                onClick={() => onFeedback(turn.turnId, 'negative')}
              >
                <ThumbsDown className="h-4 w-4" />
              </button>
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}

type RecommendationGroup = RecommendationTurnDto['recommendationGroups'][number];
type RecommendationGroupItem = RecommendationGroup['items'][number];

function RecommendationResultDisclosure({ groups }: { groups: RecommendationGroup[] }) {
  const visibleGroups = groups.filter((group) => group.items.length > 0);

  if (visibleGroups.length === 0) {
    return null;
  }

  const isDrinkDetailsOnly = visibleGroups.every((group) => group.key === 'drink-details');

  return (
    <section
      aria-label={isDrinkDetailsOnly ? 'Drink details' : 'Recommendations'}
      className="space-y-3 rounded-2xl border border-border/70 bg-background/70 p-4"
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-semibold text-foreground">
          {isDrinkDetailsOnly ? 'Drink details' : 'Recommendations'}
        </h3>
        <span className="text-xs text-muted-foreground">{formatResultCount(visibleGroups)}</span>
      </div>
      <div className="space-y-2">
        {visibleGroups.map((group) => (
          <RecommendationGroupDisclosure key={group.key} group={group} />
        ))}
      </div>
    </section>
  );
}

function RecommendationGroupDisclosure({ group }: { group: RecommendationGroup }) {
  const [isExpanded, setIsExpanded] = useState(false);
  const label = formatGroupLabel(group);

  return (
    <section aria-label={label} className="rounded-xl border border-border/60 bg-card/90">
      <button
        type="button"
        className="flex w-full items-center justify-between gap-3 px-4 py-3 text-left transition-colors hover:bg-accent/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        aria-expanded={isExpanded}
        onClick={() => setIsExpanded((current) => !current)}
      >
        <span className="flex min-w-0 flex-wrap items-center gap-2">
          <Badge variant={formatGroupBadgeVariant(group)}>{label}</Badge>
          <span className="text-xs text-muted-foreground">{formatGroupCount(group)}</span>
        </span>
        <ChevronDown
          className={cn(
            'h-4 w-4 shrink-0 text-muted-foreground transition-transform',
            isExpanded && 'rotate-180',
          )}
          aria-hidden="true"
        />
      </button>
      {isExpanded ? (
        <ul className="space-y-2 border-t border-border/60 p-3">
          {group.items.map((item) => (
            <RecommendationGroupItemDisclosure
              key={item.drinkId}
              item={item}
              groupKey={group.key}
            />
          ))}
        </ul>
      ) : null}
    </section>
  );
}

function RecommendationGroupItemDisclosure({
  item,
  groupKey,
}: {
  item: RecommendationGroupItem;
  groupKey: string;
}) {
  const [isExpanded, setIsExpanded] = useState(false);
  const recipeEntries = item.recipeEntries ?? [];
  const isDrinkDetails = groupKey === 'drink-details';
  const missingSummary =
    !isDrinkDetails && item.missingIngredientNames.length > 0
      ? `Missing ${item.missingIngredientNames.join(', ')}`
      : null;

  return (
    <li className="rounded-lg border border-border/60 bg-background/80">
      <button
        type="button"
        className="flex w-full items-start justify-between gap-3 px-4 py-3 text-left transition-colors hover:bg-accent/55 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        aria-expanded={isExpanded}
        onClick={() => setIsExpanded((current) => !current)}
      >
        <span className="min-w-0 space-y-1">
          <span className="block font-medium text-foreground">{item.drinkName}</span>
          {item.description ? (
            <span className="block text-sm leading-6 text-muted-foreground">
              {item.description}
            </span>
          ) : null}
          {missingSummary ? (
            <span className="block text-xs font-medium text-warning">{missingSummary}</span>
          ) : null}
        </span>
        <ChevronDown
          className={cn(
            'mt-1 h-4 w-4 shrink-0 text-muted-foreground transition-transform',
            isExpanded && 'rotate-180',
          )}
          aria-hidden="true"
        />
      </button>
      {isExpanded ? (
        <ul className="space-y-1 border-t border-border/60 px-4 py-3 text-sm leading-6 text-foreground">
          {recipeEntries.length > 0 ? (
            <li>
              <span className="text-muted-foreground">Recipe:</span>{' '}
              {recipeEntries.map((entry, index) => (
                <Fragment key={`${item.drinkId}-${entry.ingredientName}`}>
                  <span
                    className={entry.isOwned ? 'font-medium text-foreground' : 'text-foreground'}
                  >
                    {entry.ingredientName} ({entry.quantity})
                  </span>
                  {!entry.isOwned ? (
                    <span className="text-muted-foreground"> need to restock</span>
                  ) : null}
                  {index < recipeEntries.length - 1 ? ', ' : null}
                </Fragment>
              ))}
            </li>
          ) : null}
          {!isDrinkDetails && item.matchedSignals.length > 0 ? (
            <li>Matches: {item.matchedSignals.join(', ')}</li>
          ) : null}
          {!isDrinkDetails ? (
            item.missingIngredientNames.length > 0 ? (
              <li>Buy next: {item.missingIngredientNames.join(', ')}</li>
            ) : (
              <li>Available now: everything is already in your bar.</li>
            )
          ) : null}
        </ul>
      ) : null}
    </li>
  );
}

function formatResultCount(groups: RecommendationGroup[]) {
  const itemCount = groups.reduce((total, group) => total + group.items.length, 0);
  return itemCount === 1 ? '1 drink' : `${itemCount} drinks`;
}

function formatGroupCount(group: RecommendationGroup) {
  if (group.key === 'drink-details') {
    return 'Resolved drink';
  }

  return group.items.length === 1 ? '1 drink' : `${group.items.length} drinks`;
}

function formatGroupLabel(group: RecommendationGroup) {
  if (group.key === 'make-now') {
    return 'Available now';
  }

  if (group.key === 'buy-next') {
    return 'Buy next';
  }

  return group.label;
}

function formatGroupBadgeVariant(group: RecommendationGroup): BadgeProps['variant'] {
  if (group.key === 'make-now') {
    return 'success';
  }

  if (group.key === 'buy-next') {
    return 'warning';
  }

  return 'neutral';
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
