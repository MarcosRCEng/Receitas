import { env } from '@shared/config';
import { configureHttpAuth, httpClient } from '@shared/http';

import type {
  LoginRequest,
  RegisteredUserResponse,
  RegisterUserRequest,
  UserProfileResponse,
} from '../types';
import {
  clearStoredAuthTokens,
  getStoredAccessToken,
  getStoredAuthTokens,
  persistAuthTokens,
} from './authTokenStorage';
import type { TokensResponse } from '../types';

const LOGIN_ENDPOINT = '/login';
const REGISTER_ENDPOINT = '/user';
const CURRENT_USER_ENDPOINT = '/user';
const GOOGLE_LOGIN_ENDPOINT = '/login/google';
const REFRESH_TOKEN_ENDPOINT = '/token/refresh-token';

type SessionInvalidationListener = () => void;

const sessionInvalidationListeners = new Set<SessionInvalidationListener>();
let refreshInFlight: Promise<string | null> | null = null;

configureHttpAuth({
  getAccessToken: getStoredAccessToken,
  refreshToken: refreshAccessToken,
  onSessionInvalid: invalidateSession,
});

async function login(request: LoginRequest): Promise<RegisteredUserResponse> {
  const response = await httpClient.post<RegisteredUserResponse, LoginRequest>(
    LOGIN_ENDPOINT,
    request,
    { skipAuth: true },
  );

  return persistAuthenticatedResponse(response);
}

async function register(request: RegisterUserRequest): Promise<RegisteredUserResponse> {
  const response = await httpClient.post<RegisteredUserResponse, RegisterUserRequest>(
    REGISTER_ENDPOINT,
    request,
    { skipAuth: true },
  );

  return persistAuthenticatedResponse(response);
}

async function getCurrentUser(): Promise<UserProfileResponse> {
  const response = await httpClient.get<UserProfileResponse>(CURRENT_USER_ENDPOINT);

  if (!response) {
    throw new Error('Current user response did not include user data.');
  }

  return response;
}

function buildGoogleLoginUrl(frontendCallbackUrl = env.googleReturnUrl): string {
  const url = new URL(`${env.apiVersionedBaseUrl}${GOOGLE_LOGIN_ENDPOINT}`);
  url.searchParams.set('returnUrl', frontendCallbackUrl);

  return url.toString();
}

function logoutLocal(): void {
  clearStoredAuthTokens();
}

function subscribeToSessionInvalidation(listener: SessionInvalidationListener): () => void {
  sessionInvalidationListeners.add(listener);

  return () => {
    sessionInvalidationListeners.delete(listener);
  };
}

function refreshAccessToken(): Promise<string | null> {
  if (!refreshInFlight) {
    refreshInFlight = requestTokenRefresh().finally(() => {
      refreshInFlight = null;
    });
  }

  return refreshInFlight;
}

async function requestTokenRefresh(): Promise<string | null> {
  const refreshToken = getStoredAuthTokens()?.refreshToken;

  if (!refreshToken) {
    return null;
  }

  try {
    const tokens = await httpClient.post<TokensResponse, { refreshToken: string }>(
      REFRESH_TOKEN_ENDPOINT,
      { refreshToken },
      { skipAuth: true },
    );

    if (!tokens) {
      throw new Error('Refresh token response did not include tokens.');
    }

    persistAuthTokens(tokens);
    return tokens.accessToken;
  } catch {
    return null;
  }
}

function invalidateSession(): void {
  clearStoredAuthTokens();
  sessionInvalidationListeners.forEach((listener) => listener());
}

function persistAuthenticatedResponse(
  response: RegisteredUserResponse | undefined,
): RegisteredUserResponse {
  if (!response) {
    throw new Error('Authentication response did not include user data.');
  }

  persistAuthTokens(response.tokens);
  return response;
}

export const authService = {
  buildGoogleLoginUrl,
  getCurrentUser,
  getStoredAuthTokens,
  login,
  logoutLocal,
  register,
  subscribeToSessionInvalidation,
};
