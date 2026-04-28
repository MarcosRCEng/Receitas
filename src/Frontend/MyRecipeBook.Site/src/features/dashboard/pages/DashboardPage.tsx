import { Card, PageTitle } from '@shared/components';

export function DashboardPage() {
  return (
    <section>
      <PageTitle title="Dashboard" subtitle="Visao inicial das suas receitas e proximas acoes." />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <Card>
          <p className="text-sm font-medium text-slate-500">Receitas cadastradas</p>
          <strong className="mt-2 block text-3xl text-slate-950">0</strong>
        </Card>
        <Card>
          <p className="text-sm font-medium text-slate-500">Favoritas</p>
          <strong className="mt-2 block text-3xl text-slate-950">0</strong>
        </Card>
        <Card className="sm:col-span-2 xl:col-span-1">
          <p className="text-sm font-medium text-slate-500">Ultima atividade</p>
          <p className="mt-2 text-sm leading-6 text-slate-600">Nenhuma receita criada ainda.</p>
        </Card>
      </div>
    </section>
  );
}
