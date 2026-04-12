import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

type InlineMessageProps = {
  tone: 'success' | 'danger' | 'warning';
  message: string;
};

export function InlineMessage(props: InlineMessageProps) {
  return (
    <div
      className={cn(
        'flex items-start gap-3 rounded-xl border px-4 py-3 text-sm',
        props.tone === 'danger' && 'border-rose-200 bg-rose-50 text-rose-700',
        props.tone === 'success' && 'border-emerald-200 bg-emerald-50 text-emerald-700',
        props.tone === 'warning' && 'border-amber-200 bg-amber-50 text-amber-700',
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
