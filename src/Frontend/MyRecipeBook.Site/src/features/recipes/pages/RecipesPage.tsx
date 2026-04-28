import { Button, Card, PageTitle } from '@shared/components';

export function RecipesPage() {
  return (
    <section>
      <PageTitle
        actions={<Button>Nova receita</Button>}
        subtitle="Liste, filtre e acompanhe suas receitas cadastradas."
        title="Receitas"
      />
      <Card>
        <p className="text-sm leading-6 text-slate-600">Pagina placeholder para listagem de receitas.</p>
      </Card>
    </section>
  );
}
