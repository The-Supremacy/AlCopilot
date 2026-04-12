import { Badge } from '@/components/ui/badge';

type StatusPillProps = {
  tone: 'neutral' | 'success' | 'warning' | 'danger';
  children: string;
};

export function StatusPill(props: StatusPillProps) {
  const variant =
    props.tone === 'success'
      ? 'success'
      : props.tone === 'warning'
        ? 'warning'
        : props.tone === 'danger'
          ? 'destructive'
          : 'default';

  return <Badge variant={variant}>{props.children}</Badge>;
}
