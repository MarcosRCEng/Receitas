export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterUserRequest = {
  name: string;
  email: string;
  password: string;
};

export type TokensResponse = {
  accessToken: string;
  refreshToken: string;
};

export type RegisteredUserResponse = {
  name: string;
  tokens: TokensResponse;
};

export type ErrorResponse = {
  errors?: string[];
  message?: string;
  tokenIsExpired?: boolean;
};
