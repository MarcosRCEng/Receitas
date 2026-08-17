import { type FormEvent, useState } from 'react';
import { Link } from 'react-router-dom';

import { useAuth } from '@features/auth/providers';
import { Button, Card, Input } from '@shared/components';
import { isApiRequestError } from '@shared/http';

const REQUIRED_FIELDS_ERROR = 'Informe nome, e-mail, senha e confirmação de senha para criar sua conta.';
const PASSWORD_CONFIRMATION_ERROR = 'A confirmação de senha precisa ser igual à senha informada.';
const EMAIL_ALREADY_REGISTERED_ERROR = 'Já existe uma conta cadastrada com este e-mail.';
const NETWORK_ERROR = 'Não foi possível conectar à API. Verifique sua conexão e tente novamente.';
const DEFAULT_REGISTER_ERROR = 'Não foi possível criar sua conta agora. Tente novamente em instantes.';

export function RegisterPage() {
  const { register } = useAuth();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [passwordConfirmation, setPasswordConfirmation] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();
    const trimmedEmail = email.trim();

    if (!trimmedName || !trimmedEmail || !password || !passwordConfirmation) {
      setErrorMessage(REQUIRED_FIELDS_ERROR);
      return;
    }

    if (password !== passwordConfirmation) {
      setErrorMessage(PASSWORD_CONFIRMATION_ERROR);
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      await register({
        name: trimmedName,
        email: trimmedEmail,
        password,
      });
    } catch (error) {
      setErrorMessage(getRegisterErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-bold text-slate-950">Cadastro</h1>
      <p className="mt-2 text-sm leading-6 text-slate-600">Crie sua conta para organizar novas receitas.</p>

      <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
        <Input
          autoComplete="name"
          disabled={isSubmitting}
          label="Nome"
          name="name"
          onChange={(event) => setName(event.target.value)}
          placeholder="Seu nome"
          value={name}
        />
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
          autoComplete="new-password"
          disabled={isSubmitting}
          label="Senha"
          name="password"
          onChange={(event) => setPassword(event.target.value)}
          placeholder="Crie uma senha"
          type="password"
          value={password}
        />
        <Input
          autoComplete="new-password"
          disabled={isSubmitting}
          label="Confirmar senha"
          name="passwordConfirmation"
          onChange={(event) => setPasswordConfirmation(event.target.value)}
          placeholder="Repita a senha"
          type="password"
          value={passwordConfirmation}
        />

        {errorMessage ? (
          <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
            {errorMessage}
          </div>
        ) : null}

        <Button className="w-full" disabled={isSubmitting} type="submit">
          {isSubmitting ? 'Criando conta...' : 'Criar conta'}
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-600">
        Já tem uma conta?{' '}
        <Link
          className="font-semibold text-brand-800 underline-offset-4 hover:underline focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2"
          to="/login"
        >
          Entrar
        </Link>
      </p>
    </Card>
  );
}

function getRegisterErrorMessage(error: unknown): string {
  if (!isApiRequestError(error)) {
    return DEFAULT_REGISTER_ERROR;
  }

  if (error.isNetworkError) {
    return NETWORK_ERROR;
  }

  if (error.statusCode === 409) {
    return EMAIL_ALREADY_REGISTERED_ERROR;
  }

  if (error.statusCode === 400) {
    return error.errors[0] ?? REQUIRED_FIELDS_ERROR;
  }

  return error.errors[0] ?? DEFAULT_REGISTER_ERROR;
}
