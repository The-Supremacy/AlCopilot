import { useState, type ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import type { LucideIcon } from 'lucide-react';
import {
  AlertTriangle,
  ClipboardList,
  CupSoda,
  LayoutDashboard,
  LoaderCircle,
  LogIn,
  LogOut,
  Menu,
  ScrollText,
  ShieldAlert,
} from 'lucide-react';
import { buildManagementLoginUrl } from '@alcopilot/management-api-client';
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
import { PortalBreadcrumbs } from '@/features/navigation/PortalBreadcrumbs';
import { useLogoutManagementMutation, useManagementSession } from '@/state/auth/useManagementAuth';
import { cn } from '@/lib/utils';

type NavItem = {
  to: string;
  label: string;
  icon: LucideIcon;
  children?: Array<{ to: string; label: string }>;
};

const navItems: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  {
    to: '/catalog',
    label: 'Catalog',
    icon: CupSoda,
    children: [
      { to: '/catalog', label: 'Overview' },
      { to: '/catalog/drinks', label: 'Drinks' },
      { to: '/catalog/tags', label: 'Tags' },
      { to: '/catalog/ingredients', label: 'Ingredients' },
    ],
  },
  { to: '/imports', label: 'Imports', icon: ClipboardList },
  { to: '/audit', label: 'Audit', icon: ScrollText },
];

export function ManagementPortalLayout({ children }: { children: ReactNode }) {
  const sessionQuery = useManagementSession();
  const logoutMutation = useLogoutManagementMutation();
  const currentPath = getCurrentPath();

  function beginLogin() {
    window.location.assign(buildManagementLoginUrl(currentPath));
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
          icon={LoaderCircle}
          eyebrow="Management Access"
          title="Checking your session"
          body="The portal is verifying whether you already have a valid operator session."
          actionLabel={null}
        />
      </PortalShell>
    );
  }

  if (sessionQuery.isError) {
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
          message="The portal could not confirm your operator session. Retry the session check before entering management workflows."
        />
      </PortalShell>
    );
  }

  const session = sessionQuery.data;
  if (!session) {
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
          message="The portal did not receive a management session payload. Retry the session check before entering management workflows."
        />
      </PortalShell>
    );
  }

  const utility = session.isAuthenticated ? (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="rounded-2xl border border-border/70 bg-background/80 px-4 py-2">
        <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">Signed in</p>
        <p className="mt-1 text-sm font-medium text-foreground">
          {session.displayName ?? 'Operator session'}
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
    <Button variant="outline" className="w-full md:w-auto" onClick={beginLogin}>
      <LogIn className="mr-2 h-4 w-4" />
      Sign in
    </Button>
  );
  const mobileDrawerUtility = session.isAuthenticated ? (
    <div className="space-y-3">
      <div className="rounded-2xl border border-shell-foreground/15 bg-shell-foreground/5 px-4 py-3">
        <p className="text-xs uppercase tracking-[0.24em] text-shell-foreground/55">Signed in</p>
        <p className="mt-2 text-sm font-medium text-shell-foreground">
          {session.displayName ?? 'Operator session'}
        </p>
      </div>
      <Button
        variant="outline"
        className="w-full border-shell-foreground/20 bg-shell-foreground/5 text-shell-foreground hover:bg-shell-foreground/10 hover:text-shell-foreground"
        onClick={handleSignOut}
        loading={logoutMutation.isPending}
        loadingText="Signing out"
      >
        <LogOut className="h-4 w-4" />
        Sign out
      </Button>
    </div>
  ) : null;

  if (!session.isAuthenticated) {
    return (
      <PortalShell utility={utility}>
        <StateCard
          icon={ShieldAlert}
          eyebrow="Management Access"
          title="Sign in required"
          body="Management workflows are reserved for operator accounts. Sign in to continue to the management portal."
          actionLabel="Continue to sign in"
          onAction={beginLogin}
        />
      </PortalShell>
    );
  }

  if (!session.canAccessManagementPortal) {
    return (
      <PortalShell utility={utility}>
        <StateCard
          icon={AlertTriangle}
          eyebrow="Management Access"
          title="Management access denied"
          body="This account is authenticated, but it does not have the required management role. Ask an administrator to grant manager or admin access in Keycloak."
          actionLabel={null}
        />
      </PortalShell>
    );
  }

  return (
    <PortalShell utility={utility} mobileDrawerUtility={mobileDrawerUtility} showNavigation>
      {children}
    </PortalShell>
  );
}

function PortalShell({
  children,
  showNavigation = false,
  utility,
  mobileDrawerUtility,
}: {
  children: ReactNode;
  showNavigation?: boolean;
  utility: ReactNode;
  mobileDrawerUtility?: ReactNode;
}) {
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false);
  const environmentLabel = getEnvironmentLabel();

  return (
    <div className="min-h-screen p-4 md:p-6">
      <div className="mx-auto grid min-h-[calc(100vh-2rem)] max-w-7xl overflow-hidden rounded-[28px] border border-border/60 bg-card/72 shadow-soft backdrop-blur md:grid-cols-[280px_minmax(0,1fr)]">
        {showNavigation ? (
          <aside className="hidden border-r border-border/70 bg-shell px-5 py-6 text-shell-foreground md:block">
            <PortalNavigation />
          </aside>
        ) : (
          <aside className="hidden md:block md:border-r md:border-border/70 md:bg-shell md:px-5 md:py-6 md:text-shell-foreground">
            <PortalBrand />
          </aside>
        )}

        <div className="min-w-0 bg-card/40">
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
                      hideClose
                    >
                      <SheetTitle className="sr-only">Management navigation</SheetTitle>
                      <SheetDescription className="sr-only">
                        Navigate between management portal sections.
                      </SheetDescription>
                      <PortalNavigation
                        onNavigate={() => setIsMobileNavOpen(false)}
                        utility={mobileDrawerUtility}
                      />
                    </SheetContent>
                  </Sheet>
                ) : null}

                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-2 md:gap-3">
                    <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
                      Operator Console
                    </p>
                    <Badge variant="secondary">{environmentLabel}</Badge>
                  </div>
                  <h2 className="mt-3 max-w-[14ch] font-display text-[2rem] font-semibold leading-tight text-foreground md:mt-2 md:max-w-none md:text-2xl">
                    Catalog curation and import sync
                  </h2>
                </div>
              </div>

              <div className="hidden md:block md:shrink-0">{utility}</div>
            </div>

            {!showNavigation ? <div className="mt-4 md:hidden">{utility}</div> : null}
          </header>

          <main className="px-5 py-5 md:px-6 md:py-6">
            {showNavigation ? (
              <div className="mb-5">
                <PortalBreadcrumbs />
              </div>
            ) : null}
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}

function PortalBrand({ compact = false }: { compact?: boolean }) {
  if (compact) {
    return (
      <div className="inline-flex rounded-full border border-border/70 bg-background/80 px-3 py-1 text-xs text-muted-foreground">
        Manager access only
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <div>
        <p className="text-xs uppercase tracking-[0.32em] text-brand-malt/80">AlCopilot</p>
        <h1 className="mt-2 font-display text-2xl font-semibold">Management Portal</h1>
      </div>
      <div className="inline-flex rounded-full border border-shell-foreground/15 bg-shell-foreground/5 px-3 py-1 text-xs text-shell-foreground/75">
        Manager access only
      </div>
      <p className="text-sm leading-6 text-shell-foreground/78">
        Catalog curation, import sync review, and audit visibility for operator workflows.
      </p>
    </div>
  );
}

function PortalNavigation({
  onNavigate,
  utility,
}: {
  onNavigate?: () => void;
  utility?: ReactNode;
}) {
  return (
    <>
      <PortalBrand />
      <nav className="mt-8 grid gap-3" aria-label="Primary">
        {navItems.map((item) => {
          const Icon = item.icon;

          return (
            <div key={item.to} className="space-y-2">
              <Link
                to={item.to}
                onClick={onNavigate}
                activeOptions={item.to === '/' ? { exact: true } : undefined}
                className={cn(
                  'inline-flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm text-shell-foreground/75 transition-colors hover:bg-shell-foreground/10 hover:text-shell-foreground',
                )}
                activeProps={{
                  className:
                    'inline-flex w-full items-center gap-3 rounded-xl bg-gradient-to-r from-primary/25 to-brand-glass/20 px-4 py-3 text-sm text-shell-foreground',
                }}
              >
                <Icon className="h-4 w-4" />
                <span>{item.label}</span>
              </Link>
              {item.children ? (
                <div className="grid gap-1 pl-10">
                  {item.children.map((child) => (
                    <Link
                      key={child.to}
                      to={child.to}
                      onClick={onNavigate}
                      activeOptions={child.to === '/catalog' ? { exact: true } : undefined}
                      className={cn(
                        'rounded-lg px-3 py-2 text-xs font-medium uppercase tracking-wide text-shell-foreground/55 transition-colors hover:bg-shell-foreground/10 hover:text-shell-foreground',
                      )}
                      activeProps={{
                        className:
                          'rounded-lg bg-shell-foreground/10 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-shell-foreground',
                      }}
                    >
                      {child.label}
                    </Link>
                  ))}
                </div>
              ) : null}
            </div>
          );
        })}
      </nav>
      {utility ? (
        <div className="mt-8 border-t border-shell-foreground/10 pt-6">
          <p className="text-xs uppercase tracking-[0.28em] text-shell-foreground/55">Account</p>
          <div className="mt-4">{utility}</div>
        </div>
      ) : null}
    </>
  );
}

function StateCard({
  body,
  eyebrow,
  icon: Icon,
  title,
  actionLabel,
  onAction,
}: {
  body: string;
  eyebrow: string;
  icon: LucideIcon;
  title: string;
  actionLabel: string | null;
  onAction?: () => void;
}) {
  return (
    <section className="mx-auto max-w-2xl rounded-[28px] border border-border/70 bg-background/85 p-8 shadow-soft">
      <div className="inline-flex rounded-2xl border border-primary/20 bg-primary/10 p-3 text-primary">
        <Icon className="h-5 w-5" />
      </div>
      <p className="mt-5 text-xs uppercase tracking-[0.32em] text-muted-foreground">{eyebrow}</p>
      <h3 className="mt-3 font-display text-3xl font-semibold text-foreground">{title}</h3>
      <p className="mt-4 max-w-xl text-sm leading-7 text-muted-foreground">{body}</p>
      {actionLabel ? (
        <div className="mt-8">
          <Button onClick={onAction}>
            <LogIn className="mr-2 h-4 w-4" />
            {actionLabel}
          </Button>
        </div>
      ) : null}
    </section>
  );
}

function getCurrentPath() {
  if (typeof window === 'undefined') {
    return '/';
  }

  return `${window.location.pathname}${window.location.search}${window.location.hash}`;
}

function getEnvironmentLabel() {
  const mode = import.meta.env.MODE;

  if (mode === 'production') {
    return 'Production';
  }

  if (mode === 'development') {
    return 'Development';
  }

  return mode.charAt(0).toUpperCase() + mode.slice(1);
}
