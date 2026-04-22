import { useState } from 'react';
import { SendHorizontal } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';

type RecommendationComposerProps = {
  onSubmit: (message: string) => Promise<void>;
  isSubmitting: boolean;
};

export function RecommendationComposer({ onSubmit, isSubmitting }: RecommendationComposerProps) {
  const [message, setMessage] = useState('');

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = message.trim();

    if (!trimmed) {
      return;
    }

    await onSubmit(trimmed);
    setMessage('');
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-3">
      <label className="block text-sm font-medium text-foreground" htmlFor="recommendation-message">
        Ask a question
      </label>
      <Textarea
        id="recommendation-message"
        value={message}
        onChange={(event) => setMessage(event.target.value)}
        placeholder="Try something citrusy, low-effort, and preferably tequila-based."
        className="min-h-[88px] bg-card/90"
      />
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-muted-foreground">
          Ask about mood, ingredients on hand, spirits, effort level, or what to buy next.
        </p>
        <Button type="submit" loading={isSubmitting} loadingText="Sending">
          <SendHorizontal className="h-4 w-4" />
          Send
        </Button>
      </div>
    </form>
  );
}
