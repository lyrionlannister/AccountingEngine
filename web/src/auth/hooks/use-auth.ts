import { createContext, useContext } from 'react'
import type { AuthContextValue } from '../types/auth-context-value'

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider')
  }

  return context
}
