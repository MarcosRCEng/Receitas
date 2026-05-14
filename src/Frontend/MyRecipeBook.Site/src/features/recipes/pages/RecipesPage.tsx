import { PageHeader } from '@components/layout';
import { Badge, Input } from '@components/ui';

import { RecipeCard } from '../components';
import { recipeFilterOptions, recipeSummaries } from '../data/recipesMock';

export function RecipesPage() {
  return (
    <section className="w-full">
      <PageHeader description="Explore ideias para organizar seu dia a dia na cozinha." title="Receitas" />

      <div className="mb-6 space-y-4 rounded-lg border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
        <Input
          aria-label="Buscar receitas"
          name="recipe-search"
          placeholder="Buscar por nome, ingrediente ou tag"
          readOnly
        />

        <div className="grid gap-4 lg:grid-cols-2">
          <FilterGroup label="Categorias" options={recipeFilterOptions.categories} />
          <FilterGroup label="Dificuldade" options={recipeFilterOptions.difficulties} />
        </div>
      </div>

      <div className="grid min-w-0 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {recipeSummaries.map((recipe) => (
          <RecipeCard key={recipe.id} recipe={recipe} />
        ))}
      </div>
    </section>
  );
}

type FilterGroupProps = {
  label: string;
  options: string[];
};

function FilterGroup({ label, options }: FilterGroupProps) {
  return (
    <div className="min-w-0 space-y-2">
      <span className="block text-sm font-medium text-slate-700">{label}</span>
      <div className="flex min-w-0 flex-wrap gap-2">
        {options.map((option, index) => (
          <Badge key={option} variant={index === 0 ? 'default' : 'muted'}>
            {option}
          </Badge>
        ))}
      </div>
    </div>
  );
}
