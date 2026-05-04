import type { InputHTMLAttributes } from 'react';

import { cn } from '@shared/utils';

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string;
};

export function Input({ className, id, label, ...props }: InputProps) {
  const inputId = id ?? props.name;

  return (
    <label className="block">
      {label ? <span className="mb-2 block text-sm font-medium text-slate-700">{label}</span> : null}
      <input
        className={cn(
          'min-h-11 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm text-slate-950 shadow-sm outline-none transition placeholder:text-slate-400 focus:border-brand-500 focus:ring-2 focus:ring-brand-100 disabled:cursor-not-allowed disabled:bg-slate-100',
          className,
        )}
        id={inputId}
        {...props}
      />
    </label>
  );
}
