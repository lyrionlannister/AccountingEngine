import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { AuthLayout } from '../../auth/components/AuthLayout'
import { Alert } from '../../ui/components/Alert'
import { Button } from '../../ui/components/Button'
import { Field } from '../../ui/components/Field'
import { Input } from '../../ui/components/Input'
import { Select } from '../../ui/components/Select'
import { ApiError, firstFieldError } from '../../lib/http'
import { register } from '../utils/users-api'

export const RegisterPage = () => {
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [tenantName, setTenantName] = useState('')
  const [companyName, setCompanyName] = useState('')
  const [countryCode, setCountryCode] = useState('MX')
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({})
  const [loading, setLoading] = useState(false)

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setFieldErrors({})
    setLoading(true)
    try {
      await register({ email, password, tenantName, companyName, countryCode })
      navigate('/login', { replace: true, state: { registered: true } })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
        setFieldErrors(err.fieldErrors)
      } else {
        setError('No se pudo completar el registro. Intenta de nuevo.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthLayout title="Crea tu cuenta" description="Registra tu empresa y empieza a usar AccountingEngine.">
      {error && <Alert tone="error">{error}</Alert>}

      <form className="grid gap-4" onSubmit={onSubmit}>
        <Field id="email" label="Correo" help={firstFieldError(fieldErrors, 'email')} tone={firstFieldError(fieldErrors, 'email') ? 'error' : 'default'}>
          <Input
            id="email"
            type="email"
            autoComplete="username"
            required
            invalid={!!firstFieldError(fieldErrors, 'email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </Field>

        <Field
          id="password"
          label="Contraseña"
          help={firstFieldError(fieldErrors, 'password')}
          tone={firstFieldError(fieldErrors, 'password') ? 'error' : 'default'}
        >
          <Input
            id="password"
            type="password"
            autoComplete="new-password"
            required
            invalid={!!firstFieldError(fieldErrors, 'password')}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </Field>

        <Field
          id="tenantName"
          label="Nombre del grupo empresarial"
          help={firstFieldError(fieldErrors, 'tenantName')}
          tone={firstFieldError(fieldErrors, 'tenantName') ? 'error' : 'default'}
        >
          <Input
            id="tenantName"
            required
            invalid={!!firstFieldError(fieldErrors, 'tenantName')}
            value={tenantName}
            onChange={(e) => setTenantName(e.target.value)}
          />
        </Field>

        <Field
          id="companyName"
          label="Nombre de la empresa"
          help={firstFieldError(fieldErrors, 'companyName')}
          tone={firstFieldError(fieldErrors, 'companyName') ? 'error' : 'default'}
        >
          <Input
            id="companyName"
            required
            invalid={!!firstFieldError(fieldErrors, 'companyName')}
            value={companyName}
            onChange={(e) => setCompanyName(e.target.value)}
          />
        </Field>

        <Field id="countryCode" label="País">
          <Select id="countryCode" required value={countryCode} onChange={(e) => setCountryCode(e.target.value)}>
            <option value="MX">México</option>
            <option value="CO">Colombia</option>
          </Select>
        </Field>

        <Button type="submit" variant="primary" size="md" loading={loading}>
          Crear cuenta
        </Button>
      </form>

      <p className="text-center text-[12.5px] text-muted">
        ¿Ya tienes cuenta?{' '}
        <Link to="/login" className="text-link hover:underline">
          Inicia sesión
        </Link>
      </p>
    </AuthLayout>
  )
}
