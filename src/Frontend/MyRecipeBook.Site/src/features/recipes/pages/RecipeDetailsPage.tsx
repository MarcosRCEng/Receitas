import { useParams } from 'react-router-dom';

export function RecipeDetailsPage() {
  const { id } = useParams();

  return (
    <section>
      <h1 className="text-3xl font-bold text-slate-950">Detalhes da receita</h1>
      <p className="mt-3 text-slate-700">Pagina placeholder para a receita {id}.</p>
    </section>
  );
}
