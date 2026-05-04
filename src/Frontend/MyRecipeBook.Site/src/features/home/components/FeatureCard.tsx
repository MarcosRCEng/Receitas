type FeatureCardProps = {
  accent: string;
  description: string;
  title: string;
};

export function FeatureCard({ accent, description, title }: FeatureCardProps) {
  return (
    <article className="rounded-lg border border-orange-100 bg-white p-5 shadow-sm transition hover:-translate-y-1 hover:border-brand-200 hover:shadow-md">
      <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-md bg-brand-50 text-base font-bold text-brand-700">
        {accent}
      </div>
      <h3 className="text-lg font-bold text-slate-950">{title}</h3>
      <p className="mt-3 text-base leading-7 text-slate-600">{description}</p>
    </article>
  );
}
