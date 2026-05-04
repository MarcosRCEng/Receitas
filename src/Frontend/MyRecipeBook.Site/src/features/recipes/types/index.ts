export type RecipeSummary = {
  category: string;
  cookingTime: string;
  difficulty: 'Facil' | 'Media' | 'Dificil';
  id: string;
  servings: number;
  tags: string[];
  title: string;
};
