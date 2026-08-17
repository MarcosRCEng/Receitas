import { type ReactNode, useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { isApiRequestError } from '@shared/http';
import { Loading } from '@shared/components';

import { authService } from '../services';
import type {
  LoginRequest,
  RegisteredUserResponse,
  RegisterUserRequest,
  UserProfileResponse,
} from '../types';
import {
  AuthContext,
  type AuthContextValue,
  type AuthRedirectOptions,
  type AuthenticatedUser,
} from './authContext';

const DEFAULT_AUTHENTICATED_REDIRECT_PATH = '/recipes';
const DEFAULT_LOGOUT_REDIRECT_PATH = '/login';

type AuthState = {
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  isInitializing: boolean;
};

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const navigate = useNavigate();
  const [authState, setAuthState] = useState<AuthState>(() => ({
    user: null,
    isAuthenticated: false,
    isInitializing: true,
  }));

  useEffect(() => {
    let isMounted = true;

    const finishInitialization = (user: AuthenticatedUser | null) => {
      if (!isMounted) {
        return;
      }

      setAuthState({
        user,
        isAuthenticated: user !== null,
        isInitializing: false,
      });
    };

    const restoreSession = async () => {
      if (!authService.getStoredAuthTokens()) {
        finishInitialization(null);
        return;
      }

      try {
        const user = await authService.getCurrentUser();
        finishInitialization(toAuthenticatedUser(user));
      } catch (error) {
        if (isInvalidSessionError(error)) {
          authService.logoutLocal();
        }

        finishInitialization(null);
      }
    };

    void restoreSession();

    return () => {
      isMounted = false;
    };
  }, []);

  const authenticate = useCallback(
    async (
      action: Promise<RegisteredUserResponse>,
      options?: AuthRedirectOptions,
    ): Promise<AuthenticatedUser> => {
      const response = await action;
      const authenticatedUser = toAuthenticatedUser(response);

      setAuthState({
        user: authenticatedUser,
        isAuthenticated: true,
        isInitializing: false,
      });
      navigate(options?.redirectTo ?? DEFAULT_AUTHENTICATED_REDIRECT_PATH, { replace: true });

      return authenticatedUser;
    },
    [navigate],
  );

  const login = useCallback(
    (request: LoginRequest, options?: AuthRedirectOptions) =>
      authenticate(authService.login(request), options),
    [authenticate],
  );

  const register = useCallback(
    (request: RegisterUserRequest, options?: AuthRedirectOptions) =>
      authenticate(authService.register(request), options),
    [authenticate],
  );

  const logout = useCallback(
    (options?: AuthRedirectOptions) => {
      authService.logoutLocal();
      setAuthState({
        user: null,
        isAuthenticated: false,
        isInitializing: false,
      });
      navigate(options?.redirectTo ?? DEFAULT_LOGOUT_REDIRECT_PATH, { replace: true });
    },
    [navigate],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user: authState.user,
      isAuthenticated: authState.isAuthenticated,
      isInitializing: authState.isInitializing,
      login,
      register,
      logout,
    }),
    [authState.isAuthenticated, authState.isInitializing, authState.user, login, logout, register],
  );

  return (
    <AuthContext.Provider value={value}>
      {authState.isInitializing ? <Loading label="Restaurando sessão..." /> : children}
    </AuthContext.Provider>
  );
}

function toAuthenticatedUser(
  response: RegisteredUserResponse | UserProfileResponse,
): AuthenticatedUser {
  return {
    name: response.name,
  };
}

function isInvalidSessionError(error: unknown): boolean {
  return isApiRequestError(error) && (error.statusCode === 401 || error.statusCode === 403);
}
