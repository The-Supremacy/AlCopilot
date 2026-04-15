import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

type InlineMessageProps = {
  tone: 'success' | 'danger' | 'warning';
  message: string;
};

export function InlineMessage(props: InlineMessageProps) {
  const liveRole = props.tone === 'danger' ? 'alert' : 'status';

  return (
    <div
      role={liveRole}
      aria-live={props.tone === 'danger' ? 'assertive' : 'polite'}
      className={cn(
        'flex items-start gap-3 rounded-xl border px-4 py-3 text-sm',
        props.tone === 'danger' && 'border-destructive/25 bg-destructive-muted/80 text-destructive',
        props.tone === 'success' && 'border-success/25 bg-success-muted/80 text-success',
        props.tone === 'warning' && 'border-warning/25 bg-warning-muted/80 text-warning',
      )}
    >
      <Badge
        variant={
          props.tone === 'danger' ? 'destructive' : props.tone === 'success' ? 'success' : 'warning'
        }
      >
        {props.tone}
      </Badge>
      <p>{props.message}</p>
    </div>
  );
}
