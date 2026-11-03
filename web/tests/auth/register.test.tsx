import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { App } from '../../src/App'
import { AuthProvider } from '../../src/auth/components/AuthProvider'

const renderAtRegister = () =>
  render(
    <MemoryRouter initialEntries={['/registro']}>
      <AuthProvider>
        <App />
      </AuthProvider>
    </MemoryRouter>,
  )

const fillForm = () => {
  fireEvent.change(screen.getByLabelText('Correo'), { target: { value: 'owner@acme.test' } })
  fireEvent.change(screen.getByLabelText('Contraseña'), { target: { value: 's3cret-password' } })
  fireEvent.change(screen.getByLabelText('Nombre del grupo empresarial'), { target: { value: 'Acme' } })
  fireEvent.change(screen.getByLabelText('Nombre de la empresa'), { target: { value: 'Acme MX' } })
  fireEvent.click(screen.getByRole('button', { name: 'Crear cuenta' }))
}

describe('RegisterPage', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('registers the tenant owner and redirects to login with a success message', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            ownerPublicId: '1', tenantPublicId: '2', companyPublicId: '3', membershipPublicId: '4',
          }),
          { status: 201 },
        ),
      ),
    )

    renderAtRegister()
    fillForm()

    await waitFor(() => expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument())
    expect(screen.getByText('Tu cuenta quedó registrada. Inicia sesión para continuar.')).toBeInTheDocument()
    expect(fetch).toHaveBeenCalledWith('/api/users/register', expect.objectContaining({ method: 'POST' }))
  })

  it('shows the duplicate email error without navigating away', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({ title: 'Correo ya registrado', detail: 'Ya existe una cuenta registrada con ese correo.' }),
          { status: 409 },
        ),
      ),
    )

    renderAtRegister()
    fillForm()

    await waitFor(() =>
      expect(screen.getByRole('alert')).toHaveTextContent('Ya existe una cuenta registrada con ese correo.'),
    )
    expect(screen.getByRole('heading', { name: 'Crea tu cuenta' })).toBeInTheDocument()
  })
})
