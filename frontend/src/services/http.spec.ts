import { AxiosError, AxiosHeaders } from 'axios'
import { describe, expect, it } from 'vitest'

import { toProblemDetails } from '@/services/http'

function makeAxiosError(status: number, data?: unknown): AxiosError {
  return new AxiosError('Request failed', String(status), undefined, undefined, {
    status,
    statusText: 'Error',
    headers: new AxiosHeaders(),
    config: { headers: new AxiosHeaders() },
    data,
  })
}

describe('toProblemDetails', () => {
  it('returns the API ProblemDetails payload when present', () => {
    const problem = { title: 'Validation failed', status: 400, errors: { amount: ['Requerido'] } }
    const result = toProblemDetails(makeAxiosError(400, problem))
    expect(result).toEqual(problem)
  })

  it('falls back to a generic message when the response has no ProblemDetails body', () => {
    const result = toProblemDetails(makeAxiosError(500, undefined))
    expect(result.status).toBe(500)
    expect(result.title).toBeTruthy()
  })

  it('falls back to a network-error message for non-Axios errors', () => {
    const result = toProblemDetails(new Error('network down'))
    expect(result.status).toBe(0)
    expect(result.detail).toContain('conexión')
  })
})
