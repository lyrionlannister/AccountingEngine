import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { App } from '../../src/App'
import { AuthProvider } from '../../src/auth/components/AuthProvider'
import { SESSION_STORAGE_KEY } from '../../src/auth/constants/session-storage'

describe('Logout', () => {
  beforeEach(() => {
    sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify({ accessToken: 'token-123', memberships: [] }))
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    sessionStorage.clear()
  })

  it('clears the session and redirects to login', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })))

    render(
      <MemoryRouter initialEntries={['/panel']}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Menú de usuario' }))
    fireEvent.click(screen.getByRole('menuitem', { name: 'Cerrar sesión' }))

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument())
    expect(fetch).toHaveBeenCalledWith(
      '/api/auth/logout',
      expect.objectContaining({ method: 'POST', headers: expect.objectContaining({ Authorization: 'Bearer token-123' }) }),
    )
    expect(sessionStorage.getItem(SESSION_STORAGE_KEY)).toBeNull()
  })
})
