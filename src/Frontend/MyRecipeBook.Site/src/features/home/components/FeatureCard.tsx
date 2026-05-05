import { Card } from '@components/ui';

type FeatureCardProps = {
  accent: string;
  description: string;
  title: string;
};

export function FeatureCard({ accent, description, title }: FeatureCardProps) {
  return (
    <Card className="h-full p-5 transition hover:-translate-y-1 hover:border-brand-200 hover:shadow-md">
      <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-md bg-brand-50 text-base font-bold text-brand-800">
        {accent}
      </div>
      <h3 className="break-words text-lg font-bold text-slate-950">{title}</h3>
      <p className="mt-3 break-words text-base leading-7 text-slate-700">{description}</p>
    </Card>
  );
}
