import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { Alert } from '../../ui/components/Alert'
import { Button } from '../../ui/components/Button'
import { Field } from '../../ui/components/Field'
import { Input } from '../../ui/components/Input'
import { ApiError } from '../../lib/http'
import { AuthLayout } from '../components/AuthLayout'
import { useAuth } from '../hooks/use-auth'

export const LoginPage = () => {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation() as { state?: { registered?: boolean } }

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login({ email, password })
      navigate('/panel', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'No se pudo iniciar sesión. Intenta de nuevo.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title="Iniciar sesión" description="Accede a tu empresa en AccountingEngine.">
      {location.state?.registered && (
        <Alert tone="success">Tu cuenta quedó registrada. Inicia sesión para continuar.</Alert>
      )}
      {error && <Alert tone="error">{error}</Alert>}

      <form className="grid gap-4" onSubmit={onSubmit}>
        <Field id="email" label="Correo">
          <Input
            id="email"
            type="email"
            autoComplete="username"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </Field>

        <Field id="password" label="Contraseña">
          <Input
            id="password"
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </Field>

        <Button type="submit" variant="primary" size="md" loading={loading}>
          Iniciar sesión
        </Button>
      </form>

      <p className="text-center text-[12.5px] text-muted">
        ¿No tienes cuenta?{' '}
        <Link to="/registro" className="text-link hover:underline">
          Regístrate
        </Link>
      </p>
    </AuthLayout>
  )
}
