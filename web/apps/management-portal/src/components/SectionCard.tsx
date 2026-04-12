import type { ReactNode } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

type SectionCardProps = {
  title: string;
  description?: string;
  action?: ReactNode;
  children: ReactNode;
};

export function SectionCard(props: SectionCardProps) {
  return (
    <section>
      <Card>
        <CardHeader className="flex-row items-start justify-between gap-4">
          <div>
            <CardTitle>{props.title}</CardTitle>
            {props.description ? <CardDescription>{props.description}</CardDescription> : null}
          </div>
          {props.action ? <div>{props.action}</div> : null}
        </CardHeader>
        <CardContent>{props.children}</CardContent>
      </Card>
    </section>
  );
}
