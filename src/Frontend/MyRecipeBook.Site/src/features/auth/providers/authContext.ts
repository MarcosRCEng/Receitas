import { createContext } from 'react';

import type { LoginRequest, RegisterUserRequest } from '../types';

export type AuthenticatedUser = {
  name: string;
};

export type AuthRedirectOptions = {
  redirectTo?: string;
};

export type AuthContextValue = {
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  login: (
    request: LoginRequest,
    options?: AuthRedirectOptions,
  ) => Promise<AuthenticatedUser>;
  register: (
    request: RegisterUserRequest,
    options?: AuthRedirectOptions,
  ) => Promise<AuthenticatedUser>;
  logout: (options?: AuthRedirectOptions) => void;
};

export const AuthContext = createContext<AuthContextValue | null>(null);
