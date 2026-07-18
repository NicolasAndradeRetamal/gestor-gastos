import { httpClient } from '@/services/http'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '@/types'

export const authService = {
  async register(payload: RegisterRequest): Promise<AuthResponse> {
    const { data } = await httpClient.post<AuthResponse>('/auth/register', payload)
    return data
  },

  async login(payload: LoginRequest): Promise<AuthResponse> {
    const { data } = await httpClient.post<AuthResponse>('/auth/login', payload)
    return data
  },

  async me(): Promise<User> {
    const { data } = await httpClient.get<User>('/auth/me')
    return data
  },
}
