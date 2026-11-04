import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { App } from '../../src/app'
import { AuthProvider } from '../../src/auth/components/auth-provider'

const renderAtLogin = () =>
  render(
    <MemoryRouter initialEntries={['/login']}>
      <AuthProvider>
        <App />
      </AuthProvider>
    </MemoryRouter>,
  )

describe('LoginPage', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('logs in and redirects to the panel', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ accessToken: 'token-123', memberships: [] }), { status: 200 }),
      ),
    )

    renderAtLogin()

    fireEvent.change(screen.getByLabelText('Correo'), { target: { value: 'owner@acme.test' } })
    fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 's3cret' } })
    fireEvent.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Panel financiero' })).toBeInTheDocument())
    expect(fetch).toHaveBeenCalledWith(
      '/api/auth/login',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ email: 'owner@acme.test', password: 's3cret' }) }),
    )
  })

  it('shows the server error without navigating away', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ title: 'Credenciales inválidas' }), { status: 401 }),
      ),
    )

    renderAtLogin()

    fireEvent.change(screen.getByLabelText('Correo'), { target: { value: 'owner@acme.test' } })
    fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: 'Iniciar sesión' }))

    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Credenciales inválidas'))
    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument()
  })
})
