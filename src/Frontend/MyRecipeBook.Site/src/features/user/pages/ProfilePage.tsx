import { Card, Input, PageTitle } from '@shared/components';

export function ProfilePage() {
  return (
    <section>
      <PageTitle title="Perfil" subtitle="Dados basicos da sua conta." />
      <Card className="max-w-2xl">
        <form className="space-y-4">
          <Input label="Nome" name="name" placeholder="Seu nome" />
          <Input label="E-mail" name="email" placeholder="voce@email.com" type="email" />
        </form>
      </Card>
    </section>
  );
}
