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
    <Card className="flex h-full flex-col overflow-hidden p-0 transition hover:-translate-y-0.5 hover:border-brand-200 hover:shadow-md">
      <div className="flex aspect-[16/9] items-center justify-center bg-gradient-to-br from-brand-100 via-orange-50 to-emerald-50 text-3xl font-bold text-brand-800">
        {recipe.imagePlaceholder}
      </div>

      <CardHeader className="mb-0 p-5 pb-3">
        <div className="flex min-w-0 flex-wrap items-center gap-2">
          <Badge className="break-words">{recipe.category}</Badge>
          <Badge className="break-words" variant={difficultyVariant[recipe.difficulty]}>
            {recipe.difficulty}
          </Badge>
        </div>
        <h2 className="break-words text-xl font-bold text-slate-950">{recipe.title}</h2>
        <p className="text-sm leading-6 text-slate-600">{recipe.description}</p>
      </CardHeader>

      <CardContent className="flex-1 px-5 pb-0">
        <dl className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
          <div className="min-w-0">
            <dt className="font-medium text-slate-600">Tempo de preparo</dt>
            <dd className="mt-1 break-words font-semibold text-slate-900">
              {recipe.preparationTimeMinutes} minutos
            </dd>
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

      <CardFooter className="p-5 pt-4">
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
