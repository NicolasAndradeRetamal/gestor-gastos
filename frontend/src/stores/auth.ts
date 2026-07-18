import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { AUTH_TOKEN_KEY, toProblemDetails } from '@/services/http'
import { authService } from '@/services/auth.service'
import type { LoginRequest, ProblemDetails, RegisterRequest, User } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(AUTH_TOKEN_KEY))
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)

  const isAuthenticated = computed(() => Boolean(token.value))

  function setSession(nextToken: string, nextUser: User): void {
    token.value = nextToken
    user.value = nextUser
    localStorage.setItem(AUTH_TOKEN_KEY, nextToken)
  }

  function clearSession(): void {
    token.value = null
    user.value = null
    localStorage.removeItem(AUTH_TOKEN_KEY)
  }

  async function login(payload: LoginRequest): Promise<boolean> {
    isLoading.value = true
    error.value = null
    try {
      const response = await authService.login(payload)
      setSession(response.token, response.user)
      return true
    } catch (err) {
      error.value = toProblemDetails(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function register(payload: RegisterRequest): Promise<boolean> {
    isLoading.value = true
    error.value = null
    try {
      const response = await authService.register(payload)
      setSession(response.token, response.user)
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

  function logout(): void {
    clearSession()
  }

  return {
    token,
    user,
    isLoading,
    error,
    isAuthenticated,
    login,
    register,
    logout,
    fetchCurrentUser,
  }
})
