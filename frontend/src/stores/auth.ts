import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { AUTH_TOKEN_KEY, REFRESH_TOKEN_KEY, clearStoredTokens, setStoredTokens, toProblemDetails } from '@/services/http'
import { authService } from '@/services/auth.service'
import { isTwoFactorChallenge } from '@/types'
import type { AuthResponse, LoginRequest, ProblemDetails, RegisterRequest, User } from '@/types'

export type LoginOutcome = 'authenticated' | 'two-factor' | 'error'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(AUTH_TOKEN_KEY))
  const user = ref<User | null>(null)
  // Ephemeral 2FA challenge token: kept in memory only, never persisted.
  const twoFactorToken = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)

  const isAuthenticated = computed(() => Boolean(token.value))
  const hasTwoFactorChallenge = computed(() => Boolean(twoFactorToken.value))

  function setSession(response: AuthResponse): void {
    token.value = response.token
    user.value = response.user
    twoFactorToken.value = null
    setStoredTokens(response.token, response.refreshToken)
  }

  function clearSession(): void {
    token.value = null
    user.value = null
    twoFactorToken.value = null
    clearStoredTokens()
  }

  async function login(payload: LoginRequest): Promise<LoginOutcome> {
    isLoading.value = true
    error.value = null
    try {
      const result = await authService.login(payload)
      if (isTwoFactorChallenge(result)) {
        twoFactorToken.value = result.twoFactorToken
        return 'two-factor'
      }
      setSession(result)
      return 'authenticated'
    } catch (err) {
      error.value = toProblemDetails(err)
      return 'error'
    } finally {
      isLoading.value = false
    }
  }

  async function verifyTwoFactor(code: string): Promise<boolean> {
    if (!twoFactorToken.value) return false
    isLoading.value = true
    error.value = null
    try {
      const response = await authService.verifyTwoFactor(twoFactorToken.value, code)
      setSession(response)
      return true
    } catch (err) {
      error.value = toProblemDetails(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  function cancelTwoFactorChallenge(): void {
    twoFactorToken.value = null
  }

  async function register(payload: RegisterRequest): Promise<boolean> {
    isLoading.value = true
    error.value = null
    try {
      const response = await authService.register(payload)
      setSession(response)
      return true
    } catch (err) {
      error.value = toProblemDetails(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  // Restores `user` from a persisted token (e.g. after a page reload).
  async function fetchCurrentUser(): Promise<void> {
    if (!token.value) return
    try {
      user.value = await authService.me()
    } catch {
      clearSession()
    }
  }

  function setUser(next: User): void {
    user.value = next
  }

  async function logout(): Promise<void> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
    clearSession()
    // Best-effort server-side revocation; the local session is already gone.
    if (refreshToken) {
      try {
        await authService.logout(refreshToken)
      } catch {
        // Ignore: the refresh token expires on its own.
      }
    }
  }

  return {
    token,
    user,
    twoFactorToken,
    isLoading,
    error,
    isAuthenticated,
    hasTwoFactorChallenge,
    login,
    verifyTwoFactor,
    cancelTwoFactorChallenge,
    register,
    logout,
    fetchCurrentUser,
    setUser,
  }
})
