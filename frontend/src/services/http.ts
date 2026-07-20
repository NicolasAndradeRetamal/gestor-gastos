import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'

import type { ProblemDetails, RefreshResponse } from '@/types'

export const AUTH_TOKEN_KEY = 'gestor-gastos.token'
export const REFRESH_TOKEN_KEY = 'gestor-gastos.refreshToken'

const baseURL = import.meta.env.VITE_API_BASE_URL || '/api'

export const httpClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Auth endpoints whose own 401 is a normal outcome (bad credentials, wrong 2FA
// code, expired challenge) and must never trigger a token refresh or a redirect.
const NO_REFRESH_PATHS = ['/auth/login', '/auth/register', '/auth/refresh', '/auth/logout', '/auth/2fa/verify']

// Set by main.ts once the router exists, avoiding a circular import between
// this module and router/index.ts.
let onUnauthorized: (() => void) | null = null

export function setUnauthorizedHandler(handler: () => void): void {
  onUnauthorized = handler
}

export function setStoredTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(AUTH_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

export function clearStoredTokens(): void {
  localStorage.removeItem(AUTH_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

httpClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem(AUTH_TOKEN_KEY)
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

// Single in-flight refresh shared by all requests that hit a 401 at once.
let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
  if (!refreshToken) throw new Error('No refresh token available.')

  // A bare axios call (no interceptors) so a 401 here cannot recurse.
  const { data } = await axios.post<RefreshResponse>(
    `${baseURL}/auth/refresh`,
    { refreshToken },
    { headers: { 'Content-Type': 'application/json' } },
  )
  setStoredTokens(data.token, data.refreshToken)
  return data.token
}

function forceSignOut(): void {
  clearStoredTokens()
  onUnauthorized?.()
}

httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (InternalAxiosRequestConfig & { _retried?: boolean }) | undefined
    const isAuthPath = NO_REFRESH_PATHS.some((path) => (original?.url ?? '').includes(path))

    if (error.response?.status !== 401 || !original || isAuthPath) {
      return Promise.reject(error)
    }

    // Access token likely expired: refresh once and replay the request.
    if (!original._retried && localStorage.getItem(REFRESH_TOKEN_KEY)) {
      original._retried = true
      try {
        refreshPromise ??= refreshAccessToken().finally(() => {
          refreshPromise = null
        })
        const newToken = await refreshPromise
        original.headers.set('Authorization', `Bearer ${newToken}`)
        return await httpClient(original)
      } catch {
        forceSignOut()
        return Promise.reject(error)
      }
    }

    forceSignOut()
    return Promise.reject(error)
  },
)

const FALLBACK_PROBLEM: ProblemDetails = {
  title: 'Error inesperado',
  status: 0,
  detail: 'No se pudo contactar al servidor. Comprueba tu conexión e inténtalo de nuevo.',
}

// Normalizes any Axios/network failure into the API's ProblemDetails shape
// so every caller can render `detail`/`errors` the same way.
export function toProblemDetails(error: unknown): ProblemDetails {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ProblemDetails | undefined
    if (data && typeof data === 'object' && 'title' in data) {
      return data
    }
    if (error.response) {
      return {
        title: 'Error inesperado',
        status: error.response.status,
        detail: FALLBACK_PROBLEM.detail,
      }
    }
  }
  return FALLBACK_PROBLEM
}
