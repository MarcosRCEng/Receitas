import { useParams } from 'react-router-dom';

import { Card, PageTitle } from '@shared/components';

export function RecipeDetailsPage() {
  const { id } = useParams();

  return (
    <section>
      <PageTitle title="Detalhes da receita" subtitle={`Pagina placeholder para a receita ${id}.`} />
      <Card>
        <p className="text-sm leading-6 text-slate-600">
          As informacoes completas da receita ficarao organizadas nesta area.
        </p>
      </Card>
    </section>
  );
}
