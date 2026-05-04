import { useNavigate } from 'react-router-dom';

import { PageHeader } from '@components/layout';
import { Button } from '@components/ui';

import { RecipeCard } from '../components';
import { recipeSummaries } from '../data/mockRecipes';

export function RecipesPage() {
  const navigate = useNavigate();

  return (
    <section>
      <PageHeader
        action={<Button onClick={() => navigate('/recipes/new')}>Nova receita</Button>}
        description="Liste, filtre e acompanhe suas receitas cadastradas."
        title="Receitas"
      />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {recipeSummaries.map((recipe) => (
          <RecipeCard key={recipe.id} recipe={recipe} />
        ))}
      </div>
    </section>
  );
}
