import type { HTMLAttributes } from 'react';

import { cn } from '@shared/utils';

type BadgeVariant = 'default' | 'success' | 'warning' | 'muted';

type BadgeProps = HTMLAttributes<HTMLSpanElement> & {
  variant?: BadgeVariant;
};

const variants: Record<BadgeVariant, string> = {
  default: 'bg-brand-50 text-brand-800 ring-orange-200',
  success: 'bg-emerald-50 text-emerald-700 ring-emerald-100',
  warning: 'bg-amber-50 text-amber-800 ring-amber-200',
  muted: 'bg-slate-100 text-slate-700 ring-slate-200',
};

export function Badge({ className, variant = 'default', ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex max-w-full items-center rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ring-inset',
        variants[variant],
        className,
      )}
      {...props}
    />
  );
}
