import { Badge } from '@/components/ui/badge';

type StatusPillProps = {
  tone: 'neutral' | 'success' | 'warning' | 'danger';
  children: string;
  className?: string;
};

export function StatusPill(props: StatusPillProps) {
  const variant =
    props.tone === 'neutral'
      ? 'neutral'
      : props.tone === 'success'
        ? 'success'
        : props.tone === 'warning'
          ? 'warning'
          : props.tone === 'danger'
            ? 'destructive'
            : 'default';

  return (
    <Badge variant={variant} className={props.className}>
      {props.children}
    </Badge>
  );
}
