import { render, screen, waitFor } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { createPinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import { AxiosError, AxiosHeaders } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { authService } from '@/services/auth.service'
import LoginView from '@/views/LoginView.vue'

vi.mock('@/services/auth.service', () => ({
  authService: { login: vi.fn(), register: vi.fn(), me: vi.fn() },
}))

const mockedAuthService = vi.mocked(authService)

function setup() {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/login', name: 'login', component: LoginView },
      { path: '/register', name: 'register', component: { template: '<div>Register</div>' } },
      { path: '/', name: 'dashboard', component: { template: '<div>Dashboard</div>' } },
    ],
  })
  return { router, ...render(LoginView, { global: { plugins: [createPinia(), router] } }) }
}

describe('LoginView', () => {
  beforeEach(() => vi.clearAllMocks())

  it('renders the login form fields', async () => {
    setup()

    expect(await screen.findByLabelText(/Correo electrónico/)).toBeInTheDocument()
    expect(screen.getByLabelText(/Contraseña/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Iniciar sesión' })).toBeInTheDocument()
  })

  it('shows the generic credentials error banner on a 401 response', async () => {
    const user = userEvent.setup()
    mockedAuthService.login.mockRejectedValue(
      new AxiosError('Unauthorized', '401', undefined, undefined, {
        status: 401,
        statusText: 'Unauthorized',
        headers: new AxiosHeaders(),
        config: { headers: new AxiosHeaders() },
        data: { title: 'Unauthorized', status: 401 },
      }),
    )
    setup()

    await user.type(await screen.findByLabelText(/Correo electrónico/), 'ana@mail.com')
    await user.type(screen.getByLabelText(/Contraseña/), 'wrong-pass')
    await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Correo o contraseña incorrectos.')
  })

  it('navigates to the dashboard after a successful login', async () => {
    const user = userEvent.setup()
    mockedAuthService.login.mockResolvedValue({
      token: 'tok',
      expiresAt: '2026-07-17T13:00:00Z',
      user: { id: 'u1', email: 'ana@mail.com', displayName: 'Ana' },
    })
    const { router } = setup()

    await user.type(await screen.findByLabelText(/Correo electrónico/), 'ana@mail.com')
    await user.type(screen.getByLabelText(/Contraseña/), 'secret123')
    await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

    await waitFor(() => expect(router.currentRoute.value.name).toBe('dashboard'))
  })
})
