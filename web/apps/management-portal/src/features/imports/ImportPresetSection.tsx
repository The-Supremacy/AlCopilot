import type { FormEvent } from 'react';
import { SectionCard } from '@/components/SectionCard';
import { Button } from '@/components/ui/button';

type ImportPresetSectionProps = {
  strategyKey: string;
  onSubmit: () => Promise<void>;
  isSubmitting: boolean;
};

export function ImportPresetSection({
  strategyKey,
  onSubmit,
  isSubmitting,
}: ImportPresetSectionProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <SectionCard
      title="Import default preset"
      description="Start a new import using the preserved IBA cocktails snapshot."
    >
      <div className="space-y-4 text-sm text-muted-foreground">
        <div className="rounded-2xl border border-border bg-background/80 p-4">
          <p className="font-medium text-foreground">Preset source</p>
          <p className="mt-2">Repository: `rasmusab/iba-cocktails`</p>
          <p>Snapshot file: `iba-web/iba-cocktails-web.json`</p>
          <p>Strategy key: `{strategyKey}`</p>
        </div>

        <form onSubmit={handleSubmit}>
          <Button type="submit" loading={isSubmitting} loadingText="Starting import...">
            Import default preset
          </Button>
        </form>
      </div>
    </SectionCard>
  );
}
