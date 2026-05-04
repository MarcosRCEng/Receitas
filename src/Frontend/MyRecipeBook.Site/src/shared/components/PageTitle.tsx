import type { ReactNode } from 'react';

import { PageHeader } from '@components/layout';

type PageTitleProps = {
  actions?: ReactNode;
  subtitle?: string;
  title: string;
};

export function PageTitle({ actions, subtitle, title }: PageTitleProps) {
  return <PageHeader action={actions} description={subtitle} title={title} />;
}
