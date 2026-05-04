import type { RecipeSummary } from '../types';

export const recipeSummaries: RecipeSummary[] = [
  {
    category: 'Almoco',
    cookingTime: '35 min',
    difficulty: 'Facil',
    id: 'risoto-de-legumes',
    servings: 3,
    tags: ['Vegetariana', 'Rapida'],
    title: 'Risoto de legumes',
  },
  {
    category: 'Jantar',
    cookingTime: '50 min',
    difficulty: 'Media',
    id: 'frango-ao-molho',
    servings: 4,
    tags: ['Proteina', 'Familia'],
    title: 'Frango ao molho',
  },
  {
    category: 'Sobremesa',
    cookingTime: '45 min',
    difficulty: 'Facil',
    id: 'bolo-de-cenoura',
    servings: 8,
    tags: ['Doce', 'Favorita'],
    title: 'Bolo de cenoura',
  },
];
