import { Button, Card, Input } from '@shared/components';

export function LoginPage() {
  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-bold text-slate-950">Login</h1>
      <p className="mt-2 text-sm leading-6 text-slate-600">Entre para acessar suas receitas salvas.</p>

      <form className="mt-6 space-y-4">
        <Input label="E-mail" name="email" placeholder="voce@email.com" type="email" />
        <Input label="Senha" name="password" placeholder="Sua senha" type="password" />
        <Button className="w-full">
          Entrar
        </Button>
      </form>
    </Card>
  );
}
