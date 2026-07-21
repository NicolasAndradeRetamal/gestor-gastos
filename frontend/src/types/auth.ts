export interface User {
  id: string
  email: string
  displayName: string
  twoFactorEnabled: boolean
  twoFactorEnabledAt: string | null
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  displayName: string
}

export interface AuthResponse {
  token: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  user: User
}

export interface TwoFactorChallenge {
  twoFactorRequired: true
  twoFactorToken: string
}

export type LoginResult = AuthResponse | TwoFactorChallenge

export function isTwoFactorChallenge(result: LoginResult): result is TwoFactorChallenge {
  return 'twoFactorRequired' in result
}

export interface RefreshResponse {
  token: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

export interface TwoFactorSetupResponse {
  secret: string
  otpauthUri: string
}

export interface TwoFactorEnableResponse {
  recoveryCodes: string[]
}
