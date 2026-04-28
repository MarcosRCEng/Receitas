import { Card, Loading } from '@shared/components';

export function GoogleCallbackPage() {
  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-bold text-slate-950">Callback Google</h1>
      <p className="mt-2 text-sm leading-6 text-slate-600">
        Pagina placeholder para retorno de autenticacao Google.
      </p>
      <div className="mt-6">
        <Loading label="Validando retorno..." />
      </div>
    </Card>
  );
}
