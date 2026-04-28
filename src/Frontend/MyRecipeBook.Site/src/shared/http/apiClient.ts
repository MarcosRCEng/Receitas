import axios, { AxiosError } from 'axios';

import { env } from '@shared/config';

type ApiErrorResponse = {
  message?: string;
  errors?: string[];
};

function getAuthorizationToken(): string | null {
  return null;
}

export const apiClient = axios.create({
  baseURL: env.apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = getAuthorizationToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    const message =
      error.response?.data?.message ??
      error.response?.data?.errors?.[0] ??
      error.message ??
      'Unexpected API error.';

    return Promise.reject(new Error(message));
  },
);
