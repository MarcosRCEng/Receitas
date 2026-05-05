import type { ReactNode } from 'react';

type PageHeaderProps = {
  action?: ReactNode;
  description?: string;
  title: string;
};

export function PageHeader({ action, description, title }: PageHeaderProps) {
  return (
    <header className="mb-6 flex min-w-0 flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0">
        <h1 className="text-2xl font-bold tracking-normal text-slate-950 sm:text-3xl">{title}</h1>
        {description ? <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-700">{description}</p> : null}
      </div>
      {action ? <div className="flex flex-wrap gap-2 sm:justify-end">{action}</div> : null}
    </header>
  );
}
