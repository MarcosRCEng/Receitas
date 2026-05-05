import type { HTMLAttributes } from 'react';

import { cn } from '@shared/utils';

type CardProps = HTMLAttributes<HTMLDivElement>;

export function Card({ className, ...props }: CardProps) {
  return <div className={cn('min-w-0 rounded-lg border border-slate-200 bg-white p-6 shadow-sm', className)} {...props} />;
}

export function CardHeader({ className, ...props }: CardProps) {
  return <div className={cn('mb-4 min-w-0 space-y-2', className)} {...props} />;
}

export function CardContent({ className, ...props }: CardProps) {
  return <div className={cn('min-w-0 space-y-4', className)} {...props} />;
}

export function CardFooter({ className, ...props }: CardProps) {
  return <div className={cn('mt-5 flex flex-wrap items-center gap-2', className)} {...props} />;
}
