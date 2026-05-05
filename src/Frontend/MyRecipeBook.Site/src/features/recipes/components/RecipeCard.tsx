import { Link } from 'react-router-dom';

import { Badge, Card, CardContent, CardFooter, CardHeader } from '@components/ui';

import type { RecipeSummary } from '../types';

type RecipeCardProps = {
  recipe: RecipeSummary;
};

const difficultyVariant: Record<RecipeSummary['difficulty'], 'success' | 'warning' | 'muted'> = {
  Facil: 'success',
  Media: 'warning',
  Dificil: 'muted',
};

export function RecipeCard({ recipe }: RecipeCardProps) {
  return (
    <Card className="flex h-full flex-col p-5 transition hover:-translate-y-0.5 hover:border-brand-200 hover:shadow-md">
      <CardHeader>
        <div className="flex min-w-0 flex-wrap items-center gap-2">
          <Badge className="break-words">{recipe.category}</Badge>
          <Badge className="break-words" variant={difficultyVariant[recipe.difficulty]}>
            {recipe.difficulty}
          </Badge>
        </div>
        <h2 className="break-words text-xl font-bold text-slate-950">{recipe.title}</h2>
      </CardHeader>

      <CardContent className="flex-1">
        <dl className="grid grid-cols-2 gap-3 text-sm">
          <div className="min-w-0">
            <dt className="font-medium text-slate-600">Tempo</dt>
            <dd className="mt-1 break-words font-semibold text-slate-900">{recipe.cookingTime}</dd>
          </div>
          <div className="min-w-0">
            <dt className="font-medium text-slate-600">Porções</dt>
            <dd className="mt-1 break-words font-semibold text-slate-900">{recipe.servings}</dd>
          </div>
        </dl>
        <div className="flex min-w-0 flex-wrap gap-2">
          {recipe.tags.map((tag) => (
            <Badge className="break-words" key={tag} variant="muted">
              {tag}
            </Badge>
          ))}
        </div>
      </CardContent>

      <CardFooter>
        <Link
          className="inline-flex min-h-9 items-center justify-center rounded-md px-3 py-1.5 text-sm font-semibold text-brand-800 transition hover:bg-brand-50 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2"
          to={`/recipes/${recipe.id}`}
        >
          Ver detalhes
        </Link>
      </CardFooter>
    </Card>
  );
}
