import { Button, Card, Input } from '@shared/components';

export function RegisterPage() {
  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-bold text-slate-950">Cadastro</h1>
      <p className="mt-2 text-sm leading-6 text-slate-600">Crie sua conta para organizar novas receitas.</p>

      <form className="mt-6 space-y-4">
        <Input label="Nome" name="name" placeholder="Seu nome" />
        <Input label="E-mail" name="email" placeholder="voce@email.com" type="email" />
        <Input label="Senha" name="password" placeholder="Crie uma senha" type="password" />
        <Button className="w-full">
          Criar conta
        </Button>
      </form>
    </Card>
  );
}
