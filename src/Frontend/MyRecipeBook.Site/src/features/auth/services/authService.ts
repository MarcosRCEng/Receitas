import { env } from '@shared/config';
import { configureHttpAuth, httpClient } from '@shared/http';

import type { LoginRequest, RegisteredUserResponse, RegisterUserRequest } from '../types';
import {
  clearStoredAuthTokens,
  getStoredAccessToken,
  getStoredAuthTokens,
  persistAuthTokens,
} from './authTokenStorage';

const LOGIN_ENDPOINT = '/login';
const REGISTER_ENDPOINT = '/user';
const GOOGLE_LOGIN_ENDPOINT = '/login/google';

configureHttpAuth({
  getAccessToken: getStoredAccessToken,
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

function buildGoogleLoginUrl(frontendCallbackUrl = env.googleReturnUrl): string {
  const url = new URL(`${env.apiVersionedBaseUrl}${GOOGLE_LOGIN_ENDPOINT}`);
  url.searchParams.set('returnUrl', frontendCallbackUrl);

  return url.toString();
}

function logoutLocal(): void {
  clearStoredAuthTokens();
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
  getStoredAuthTokens,
  login,
  logoutLocal,
  register,
};
