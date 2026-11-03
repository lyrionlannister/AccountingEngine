import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { beforeEach, describe, expect, it } from 'vitest'
import { AuthProvider } from '../src/auth/components/AuthProvider'
import { SESSION_STORAGE_KEY } from '../src/auth/constants/session-storage'
import { App } from '../src/App'

describe('App', () => {
  beforeEach(() => {
    sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify({ accessToken: 'token', memberships: [] }))
  })

  it('redirects to the panel and renders the layout chrome when authenticated', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Panel financiero' })).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: 'Pólizas' }).length).toBeGreaterThan(0)
  })

  it('redirects to login when there is no session', () => {
    sessionStorage.clear()

    render(
      <MemoryRouter initialEntries={['/panel']}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument()
  })
})
