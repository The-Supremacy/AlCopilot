import { Link } from '@tanstack/react-router';
import type { RecommendationSessionSummaryDto } from '@alcopilot/customer-api-client';
import { ChevronDown, ChevronUp, MessageSquareText, Sparkles } from 'lucide-react';
import { useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { formatRelativeDate } from '@/lib/format';
import { cn } from '@/lib/utils';

const COMPACT_SESSION_COUNT = 8;

type RecommendationSessionRailProps = {
  sessions: RecommendationSessionSummaryDto[];
  activeSessionId?: string;
  onNavigate?: () => void;
};

export function RecommendationSessionRail({
  sessions,
  activeSessionId,
  onNavigate,
}: RecommendationSessionRailProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const hiddenSessionCount = Math.max(0, sessions.length - COMPACT_SESSION_COUNT);
  const visibleSessions = isExpanded ? sessions : sessions.slice(0, COMPACT_SESSION_COUNT);

  return (
    <div className="space-y-6">
      <div className="space-y-3">
        <Badge variant="secondary" className="bg-shell-foreground/10 text-shell-foreground">
          Chat-first workspace
        </Badge>
        <div>
          <h2 className="font-display text-2xl text-shell-foreground">AlCopilot</h2>
          <p className="mt-2 text-sm leading-6 text-shell-foreground/70">
            Keep recent recommendation threads close and revisit them without leaving the shell.
          </p>
        </div>
        <Button
          asChild
          className="w-full bg-shell-foreground text-shell hover:bg-shell-foreground/90"
        >
          <Link to="/" onClick={onNavigate}>
            <Sparkles className="h-4 w-4" />
            Start a new chat
          </Link>
        </Button>
      </div>

      <nav aria-label="Recent chat sessions" className="space-y-2">
        {sessions.length === 0 ? (
          <div className="rounded-2xl border border-shell-foreground/15 bg-shell-foreground/5 p-4 text-sm text-shell-foreground/70">
            Your session history will appear here after the first recommendation reply lands.
          </div>
        ) : (
          <>
            {visibleSessions.map((session) => (
              <Link
                key={session.sessionId}
                to="/chat/$sessionId"
                params={{ sessionId: session.sessionId }}
                onClick={onNavigate}
                className={cn(
                  'block rounded-2xl border border-shell-foreground/10 bg-shell-foreground/5 p-4 transition hover:bg-shell-foreground/10',
                  activeSessionId === session.sessionId &&
                    'border-shell-foreground/35 bg-shell-foreground/12',
                )}
              >
                <div className="flex items-center gap-2 text-shell-foreground">
                  <MessageSquareText className="h-4 w-4" />
                  <span className="line-clamp-1 text-sm font-medium">{session.title}</span>
                </div>
                <p className="mt-2 line-clamp-2 text-xs leading-5 text-shell-foreground/70">
                  {session.lastAssistantMessage || 'No assistant reply yet'}
                </p>
                <p className="mt-3 text-[11px] uppercase tracking-[0.18em] text-shell-foreground/55">
                  {formatRelativeDate(session.updatedAtUtc)}
                </p>
              </Link>
            ))}
            {hiddenSessionCount > 0 ? (
              <Button
                type="button"
                variant="ghost"
                className="w-full justify-center border border-shell-foreground/10 bg-shell-foreground/5 text-shell-foreground hover:bg-shell-foreground/10 hover:text-shell-foreground"
                aria-expanded={isExpanded}
                onClick={() => setIsExpanded((current) => !current)}
              >
                {isExpanded ? (
                  <>
                    <ChevronUp className="h-4 w-4" />
                    Show fewer chats
                  </>
                ) : (
                  <>
                    <ChevronDown className="h-4 w-4" />
                    Show {hiddenSessionCount} more chats
                  </>
                )}
              </Button>
            ) : null}
          </>
        )}
      </nav>
    </div>
  );
}
