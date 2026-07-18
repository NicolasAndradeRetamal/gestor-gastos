import { createPinia, setActivePinia } from 'pinia'
import { AxiosError, AxiosHeaders } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { authService } from '@/services/auth.service'
import { AUTH_TOKEN_KEY } from '@/services/http'
import { useAuthStore } from '@/stores/auth'
import type { AuthResponse } from '@/types'

vi.mock('@/services/auth.service', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn(),
    me: vi.fn(),
  },
}))

const mockedAuthService = vi.mocked(authService)

const authResponse: AuthResponse = {
  token: 'token-123',
  expiresAt: '2026-07-17T13:00:00Z',
  user: { id: 'u1', email: 'ana@mail.com', displayName: 'Ana' },
}

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('starts unauthenticated when there is no persisted token', () => {
    const store = useAuthStore()
    expect(store.isAuthenticated).toBe(false)
  })

  it('persists the token and user on successful login', async () => {
    mockedAuthService.login.mockResolvedValue(authResponse)
    const store = useAuthStore()

    const ok = await store.login({ email: 'ana@mail.com', password: 'secret123' })

    expect(ok).toBe(true)
    expect(store.isAuthenticated).toBe(true)
    expect(store.user).toEqual(authResponse.user)
    expect(localStorage.getItem(AUTH_TOKEN_KEY)).toBe('token-123')
  })

  it('exposes a ProblemDetails error and stays unauthenticated on invalid credentials', async () => {
    const error = new AxiosError('Unauthorized', '401', undefined, undefined, {
      status: 401,
      statusText: 'Unauthorized',
      headers: new AxiosHeaders(),
      config: { headers: new AxiosHeaders() },
      data: { title: 'Unauthorized', status: 401 },
    })
    mockedAuthService.login.mockRejectedValue(error)
    const store = useAuthStore()

    const ok = await store.login({ email: 'ana@mail.com', password: 'wrong' })

    expect(ok).toBe(false)
    expect(store.isAuthenticated).toBe(false)
    expect(store.error?.status).toBe(401)
  })

  it('clears the session on logout', async () => {
    mockedAuthService.login.mockResolvedValue(authResponse)
    const store = useAuthStore()
    await store.login({ email: 'ana@mail.com', password: 'secret123' })

    store.logout()

    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
    expect(localStorage.getItem(AUTH_TOKEN_KEY)).toBeNull()
  })
})
