import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'

import type { ProblemDetails } from '@/types'

export const AUTH_TOKEN_KEY = 'gestor-gastos.token'

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Set by main.ts once the router exists, avoiding a circular import between
// this module and router/index.ts.
let onUnauthorized: (() => void) | null = null

export function setUnauthorizedHandler(handler: () => void): void {
  onUnauthorized = handler
}

httpClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem(AUTH_TOKEN_KEY)
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem(AUTH_TOKEN_KEY)
      onUnauthorized?.()
    }
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
