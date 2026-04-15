import { ExternalLink, LogOut } from 'lucide-react';
import { SectionCard } from '@/components/SectionCard';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { useLogoutCustomerMutation, useCustomerSession } from '@/state/auth/useCustomerAuth';

export function AccountPage() {
  const sessionQuery = useCustomerSession();
  const logoutMutation = useLogoutCustomerMutation();
  const session = sessionQuery.data;

  return (
    <SectionCard
      title="Account"
      description="Keep this area session-oriented. Identity administration still belongs to the identity provider."
    >
      <div className="grid gap-5 lg:grid-cols-2">
        <div className="rounded-2xl border border-border/70 bg-background/70 p-5">
          <Badge variant="secondary">Current session</Badge>
          <h3 className="mt-4 font-display text-2xl text-foreground">
            {session?.displayName ?? 'Customer account'}
          </h3>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Roles: {session?.roles.join(', ') || 'No roles returned'}
          </p>
          <Button
            className="mt-5"
            variant="outline"
            onClick={() => logoutMutation.mutateAsync()}
            loading={logoutMutation.isPending}
            loadingText="Signing out"
          >
            <LogOut className="h-4 w-4" />
            Sign out
          </Button>
        </div>

        <div className="rounded-2xl border border-border/70 bg-background/70 p-5">
          <Badge variant="secondary">Identity provider</Badge>
          <h3 className="mt-4 font-display text-2xl text-foreground">
            Manage account outside the portal
          </h3>
          <p className="mt-3 text-sm leading-6 text-muted-foreground">
            Password resets, email changes, and other identity settings remain delegated to
            Keycloak.
          </p>
          <Button asChild className="mt-5">
            <a href="/api/auth/customer/login?returnUrl=/account">
              <ExternalLink className="h-4 w-4" />
              Refresh identity session
            </a>
          </Button>
        </div>
      </div>
    </SectionCard>
  );
}
