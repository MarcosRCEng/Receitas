import { Button, Card, Input, PageTitle } from '@shared/components';

export function NewRecipePage() {
  return (
    <section>
      <PageTitle title="Nova receita" subtitle="Comece o cadastro com as informacoes principais." />
      <Card className="max-w-2xl">
        <form className="space-y-4">
          <Input label="Titulo" name="title" placeholder="Ex.: Bolo de cenoura" />
          <Input label="Tempo de preparo" name="cookingTime" placeholder="Ex.: 45 minutos" />
          <Button>Salvar rascunho</Button>
        </form>
      </Card>
    </section>
  );
}
