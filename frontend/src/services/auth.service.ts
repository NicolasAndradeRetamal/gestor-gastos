import { httpClient } from '@/services/http'
import type {
  AuthResponse,
  LoginRequest,
  LoginResult,
  RegisterRequest,
  TwoFactorEnableResponse,
  TwoFactorSetupResponse,
  User,
} from '@/types'

export const authService = {
  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const { data } = await httpClient.post<AuthResponse>('/auth/register', payload)
    return data
  },

  async login(payload: LoginRequest): Promise<LoginResult> {
    const { data } = await httpClient.post<LoginResult>('/auth/login', payload)
    return data
  },

  async me(): Promise<User> {
    const { data } = await httpClient.get<User>('/auth/me')
    return data
  },

  async logout(refreshToken: string): Promise<void> {
    await httpClient.post('/auth/logout', { refreshToken })
  },

  async verifyTwoFactor(twoFactorToken: string, code: string): Promise<AuthResponse> {
    const { data } = await httpClient.post<AuthResponse>('/auth/2fa/verify', { twoFactorToken, code })
    return data
  },

  async setupTwoFactor(): Promise<TwoFactorSetupResponse> {
    const { data } = await httpClient.post<TwoFactorSetupResponse>('/auth/2fa/setup')
    return data
  },

  async enableTwoFactor(code: string): Promise<TwoFactorEnableResponse> {
    const { data } = await httpClient.post<TwoFactorEnableResponse>('/auth/2fa/enable', { code })
    return data
  },

  async disableTwoFactor(password: string, code: string): Promise<void> {
    await httpClient.post('/auth/2fa/disable', { password, code })
  },
}
