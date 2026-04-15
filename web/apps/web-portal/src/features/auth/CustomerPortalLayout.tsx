import { useState, type ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { LoaderCircle, LogIn, LogOut, Menu, ShieldAlert, UserRoundPlus } from 'lucide-react';
import { buildCustomerLoginUrl, buildCustomerRegisterUrl } from '@alcopilot/customer-api-client';
import { InlineMessage } from '@/components/InlineMessage';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import { RecommendationSessionRail } from '@/features/recommendations/RecommendationSessionRail';
import { useRecommendationSessions } from '@/features/recommendations/hooks';
import { useLogoutCustomerMutation, useCustomerSession } from '@/state/auth/useCustomerAuth';

type NavigationItem = {
  to: '/' | '/my-bar' | '/preferences' | '/history' | '/account';
  label: string;
};

const navigationItems: NavigationItem[] = [
  { to: '/', label: 'Chat' },
  { to: '/my-bar', label: 'My Bar' },
  { to: '/preferences', label: 'Preferences' },
  { to: '/history', label: 'History' },
  { to: '/account', label: 'Account' },
];

export function CustomerPortalLayout({ children }: { children: ReactNode }) {
  const sessionQuery = useCustomerSession();
  const sessionsQuery = useRecommendationSessions();
  const logoutMutation = useLogoutCustomerMutation();
  const currentPath = getCurrentPath();
  const activeSessionId = getActiveSessionId();

  function beginLogin() {
    window.location.assign(buildCustomerLoginUrl(currentPath));
  }

  function beginRegister() {
    window.location.assign(buildCustomerRegisterUrl(currentPath));
  }

  async function handleSignOut() {
    await logoutMutation.mutateAsync();
  }

  if (sessionQuery.isLoading) {
    return (
      <PortalShell
        utility={
          <div className="inline-flex items-center gap-2 rounded-full border border-border/70 px-3 py-2 text-sm text-muted-foreground">
            <LoaderCircle className="h-4 w-4 animate-spin" />
            <span>Checking session</span>
          </div>
        }
      >
        <StateCard
          title="Checking your customer session"
          body="The portal is confirming whether you already have access before loading chat and profile data."
          action={null}
        />
      </PortalShell>
    );
  }

  if (sessionQuery.isError || !sessionQuery.data) {
    return (
      <PortalShell
        utility={
          <Button variant="outline" onClick={() => sessionQuery.refetch()}>
            Retry session check
          </Button>
        }
      >
        <InlineMessage
          tone="danger"
          message="The portal could not confirm your customer session. Retry before entering recommendation flows."
        />
      </PortalShell>
    );
  }

  const session = sessionQuery.data;

  const utility = session.isAuthenticated ? (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="rounded-2xl border border-border/70 bg-background/80 px-4 py-2">
        <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">Signed in</p>
        <p className="mt-1 text-sm font-medium text-foreground">
          {session.displayName ?? 'Customer session'}
        </p>
      </div>
      <Button
        variant="outline"
        className="w-full sm:w-auto"
        onClick={handleSignOut}
        loading={logoutMutation.isPending}
        loadingText="Signing out"
      >
        <LogOut className="h-4 w-4" />
        Sign out
      </Button>
    </div>
  ) : (
    <div className="flex flex-col gap-2 sm:flex-row">
      <Button variant="outline" onClick={beginLogin}>
        <LogIn className="h-4 w-4" />
        Sign in
      </Button>
      <Button onClick={beginRegister}>
        <UserRoundPlus className="h-4 w-4" />
        Create account
      </Button>
    </div>
  );

  if (!session.isAuthenticated) {
    return (
      <PortalShell utility={utility}>
        <StateCard
          title="Sign in required"
          body="This portal keeps recommendation chat, preferences, and your home bar behind a consistent local sign-in-required state before redirecting to Keycloak."
          action={
            <div className="flex flex-col gap-3 sm:flex-row">
              <Button onClick={beginLogin}>
                <LogIn className="h-4 w-4" />
                Continue to sign in
              </Button>
              <Button variant="outline" onClick={beginRegister}>
                <UserRoundPlus className="h-4 w-4" />
                Register first
              </Button>
            </div>
          }
        />
      </PortalShell>
    );
  }

  if (!session.canAccessCustomerPortal) {
    return (
      <PortalShell utility={utility}>
        <StateCard
          title="Customer access denied"
          body="This account is authenticated, but it does not currently have the customer user role required to enter the portal."
          action={null}
        />
      </PortalShell>
    );
  }

  return (
    <PortalShell
      utility={utility}
      showNavigation
      sessions={sessionsQuery.data ?? []}
      activeSessionId={activeSessionId}
    >
      {children}
    </PortalShell>
  );
}

function PortalShell({
  children,
  utility,
  showNavigation = false,
  sessions = [],
  activeSessionId,
}: {
  children: ReactNode;
  utility: ReactNode;
  showNavigation?: boolean;
  sessions?: Array<{
    sessionId: string;
    title: string;
    createdAtUtc: string;
    updatedAtUtc: string;
    lastAssistantMessage: string;
  }>;
  activeSessionId?: string;
}) {
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false);

  return (
    <div className="min-h-screen p-4 md:p-6">
      <div className="mx-auto grid min-h-[calc(100vh-2rem)] max-w-7xl overflow-hidden rounded-[32px] border border-border/60 bg-card/72 shadow-soft backdrop-blur md:grid-cols-[300px_minmax(0,1fr)]">
        {showNavigation ? (
          <aside className="hidden border-r border-border/70 bg-shell px-5 py-6 text-shell-foreground md:block">
            <RecommendationSessionRail sessions={sessions} activeSessionId={activeSessionId} />
          </aside>
        ) : (
          <aside className="hidden border-r border-border/70 bg-shell px-5 py-6 text-shell-foreground md:block">
            <BrandPanel />
          </aside>
        )}

        <div className="min-w-0 bg-card/45">
          <header className="border-b border-border/70 px-5 py-4 md:px-6 md:py-5">
            <div className="flex items-start justify-between gap-4">
              <div className="flex min-w-0 items-start gap-3">
                {showNavigation ? (
                  <Sheet open={isMobileNavOpen} onOpenChange={setIsMobileNavOpen}>
                    <SheetTrigger asChild>
                      <Button
                        variant="outline"
                        size="sm"
                        className="mt-1 md:hidden"
                        aria-label="Open navigation menu"
                      >
                        <Menu className="h-4 w-4" />
                      </Button>
                    </SheetTrigger>
                    <SheetContent
                      side="left"
                      className="h-screen border-0 border-r border-border/70 bg-shell p-6 text-shell-foreground"
                    >
                      <SheetTitle className="sr-only">Customer navigation</SheetTitle>
                      <SheetDescription className="sr-only">
                        Navigate between chat, profile, history, and account areas.
                      </SheetDescription>
                      <RecommendationSessionRail
                        sessions={sessions}
                        activeSessionId={activeSessionId}
                        onNavigate={() => setIsMobileNavOpen(false)}
                      />
                    </SheetContent>
                  </Sheet>
                ) : null}

                <div className="min-w-0">
                  <Badge variant="secondary">Customer Portal</Badge>
                  <h1 className="mt-3 font-display text-3xl text-foreground">
                    Chat-first drinks guidance
                  </h1>
                  <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">
                    Move between recommendation chat, your bar, and taste preferences without losing
                    the thread.
                  </p>
                </div>
              </div>
              <div className="shrink-0">{utility}</div>
            </div>

            {showNavigation ? (
              <nav aria-label="Primary navigation" className="mt-5 flex flex-wrap gap-2">
                {navigationItems.map((item) => (
                  <Button key={item.to} asChild variant="ghost" size="sm">
                    <Link to={item.to}>{item.label}</Link>
                  </Button>
                ))}
              </nav>
            ) : null}
          </header>

          <main className="min-w-0 px-5 py-5 md:px-6 md:py-6">{children}</main>
        </div>
      </div>
    </div>
  );
}

function BrandPanel() {
  return (
    <div className="space-y-5">
      <Badge variant="secondary" className="bg-shell-foreground/10 text-shell-foreground">
        Customer Portal
      </Badge>
      <div>
        <h2 className="font-display text-3xl text-shell-foreground">AlCopilot</h2>
        <p className="mt-3 text-sm leading-6 text-shell-foreground/70">
          Taste-driven recommendations with clear make-now versus buy-next structure and profile
          context.
        </p>
      </div>
      <div className="rounded-2xl border border-shell-foreground/15 bg-shell-foreground/5 p-4 text-sm text-shell-foreground/70">
        Signed-in gating stays local first, then hands off to the identity provider when you choose
        to continue.
      </div>
    </div>
  );
}

function StateCard({ title, body, action }: { title: string; body: string; action: ReactNode }) {
  return (
    <div className="mx-auto max-w-2xl rounded-[28px] border border-border/70 bg-background/90 p-8 shadow-soft">
      <div className="inline-flex rounded-full bg-primary/10 p-3 text-primary">
        <ShieldAlert className="h-6 w-6" />
      </div>
      <h2 className="mt-6 font-display text-3xl text-foreground">{title}</h2>
      <p className="mt-4 text-sm leading-7 text-muted-foreground">{body}</p>
      {action ? <div className="mt-8">{action}</div> : null}
    </div>
  );
}

function getCurrentPath() {
  if (typeof window === 'undefined') {
    return '/';
  }

  return `${window.location.pathname}${window.location.search}` || '/';
}

function getActiveSessionId() {
  if (typeof window === 'undefined') {
    return undefined;
  }

  const match = window.location.pathname.match(/^\/chat\/([^/]+)$/);
  return match?.[1];
}
