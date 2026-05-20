import { type FormEvent, useState } from 'react';

import { useAuth } from '@features/auth/providers';
import { authService } from '@features/auth/services';
import { Button, Card, Input } from '@shared/components';
import { isApiRequestError } from '@shared/http';

const REQUIRED_FIELDS_ERROR = 'Informe e-mail e senha para entrar.';
const INVALID_CREDENTIALS_ERROR = 'E-mail ou senha inválidos. Confira os dados e tente novamente.';
const NETWORK_ERROR = 'Não foi possível conectar à API. Verifique sua conexão e tente novamente.';
const DEFAULT_LOGIN_ERROR = 'Não foi possível fazer login agora. Tente novamente em instantes.';

export function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!email.trim() || !password) {
      setErrorMessage(REQUIRED_FIELDS_ERROR);
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      await login({
        email: email.trim(),
        password,
      });
    } catch (error) {
      setErrorMessage(getLoginErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleGoogleLogin() {
    window.location.assign(authService.buildGoogleLoginUrl());
  }

  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-bold text-slate-950">Login</h1>
      <p className="mt-2 text-sm leading-6 text-slate-600">Entre para acessar suas receitas salvas.</p>

      <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
        <Input
          autoComplete="email"
          disabled={isSubmitting}
          label="E-mail"
          name="email"
          onChange={(event) => setEmail(event.target.value)}
          placeholder="voce@email.com"
          type="email"
          value={email}
        />
        <Input
          autoComplete="current-password"
          disabled={isSubmitting}
          label="Senha"
          name="password"
          onChange={(event) => setPassword(event.target.value)}
          placeholder="Sua senha"
          type="password"
          value={password}
        />

        {errorMessage ? (
          <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
            {errorMessage}
          </div>
        ) : null}

        <Button className="w-full" disabled={isSubmitting} type="submit">
          {isSubmitting ? 'Entrando...' : 'Entrar'}
        </Button>
        <Button
          className="w-full"
          disabled={isSubmitting}
          onClick={handleGoogleLogin}
          type="button"
          variant="secondary"
        >
          Entrar com Google
        </Button>
      </form>
    </Card>
  );
}

function getLoginErrorMessage(error: unknown): string {
  if (!isApiRequestError(error)) {
    return DEFAULT_LOGIN_ERROR;
  }

  if (error.isNetworkError) {
    return NETWORK_ERROR;
  }

  if (error.statusCode === 400 || error.statusCode === 401) {
    return INVALID_CREDENTIALS_ERROR;
  }

  return error.errors[0] ?? DEFAULT_LOGIN_ERROR;
}
