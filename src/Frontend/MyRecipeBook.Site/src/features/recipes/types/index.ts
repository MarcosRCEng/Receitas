export type RecipeSummary = {
  category: string;
  description: string;
  difficulty: 'Facil' | 'Media' | 'Dificil';
  id: string;
  imagePlaceholder: string;
  preparationTimeMinutes: number;
  tags: string[];
  title: string;
};
