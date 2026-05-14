import type { RecipeSummary } from '../types';

export const recipeFilterOptions = {
  categories: ['Todas', 'Massas', 'Sobremesas', 'Proteinas', 'Saladas', 'Arroz'],
  difficulties: ['Todas', 'Facil', 'Media', 'Dificil'],
};

export const recipeSummaries: RecipeSummary[] = [
  {
    category: 'Massas',
    description: 'Camadas de massa, molho de tomate e queijo para um almoco em familia.',
    difficulty: 'Media',
    id: 'lasanha-caseira',
    imagePlaceholder: 'LC',
    preparationTimeMinutes: 75,
    tags: ['Forno', 'Familia', 'Conforto'],
    title: 'Lasanha caseira',
  },
  {
    category: 'Sobremesas',
    description: 'Massa fofinha com cobertura de chocolate para acompanhar o cafe.',
    difficulty: 'Facil',
    id: 'bolo-de-cenoura',
    imagePlaceholder: 'BC',
    preparationTimeMinutes: 55,
    tags: ['Doce', 'Cafe da tarde', 'Favorita'],
    title: 'Bolo de cenoura',
  },
  {
    category: 'Proteinas',
    description: 'Peito de frango temperado e dourado, ideal para refeicoes leves.',
    difficulty: 'Facil',
    id: 'frango-grelhado',
    imagePlaceholder: 'FG',
    preparationTimeMinutes: 30,
    tags: ['Rapida', 'Leve', 'Dia a dia'],
    title: 'Frango grelhado',
  },
  {
    category: 'Cafe da manha',
    description: 'Panquecas macias com poucos ingredientes para uma rotina pratica.',
    difficulty: 'Facil',
    id: 'panqueca-simples',
    imagePlaceholder: 'PS',
    preparationTimeMinutes: 25,
    tags: ['Pratica', 'Frigideira', 'Basica'],
    title: 'Panqueca simples',
  },
  {
    category: 'Saladas',
    description: 'Legumes frescos, folhas e queijo em uma combinacao colorida e leve.',
    difficulty: 'Facil',
    id: 'salada-mediterranea',
    imagePlaceholder: 'SM',
    preparationTimeMinutes: 20,
    tags: ['Vegetariana', 'Fresca', 'Sem forno'],
    title: 'Salada mediterranea',
  },
  {
    category: 'Arroz',
    description: 'Risoto cremoso com cogumelos salteados para um jantar especial.',
    difficulty: 'Dificil',
    id: 'risoto-de-cogumelos',
    imagePlaceholder: 'RC',
    preparationTimeMinutes: 50,
    tags: ['Vegetariana', 'Cremosa', 'Jantar'],
    title: 'Risoto de cogumelos',
  },
];
