import { useCallback, useState } from 'react'
import type { ReactNode } from 'react'
import { login as loginRequest, logout as logoutRequest } from '../utils/auth-api'
import { clearStoredSession, readStoredSession, writeStoredSession } from '../utils/session-storage'
import { AuthContext } from '../hooks/use-auth'
import type { LoginRequest, Session } from '../types/auth'

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [session, setSession] = useState<Session | null>(() => readStoredSession())

  const login = useCallback(async (request: LoginRequest) => {
    const response = await loginRequest(request)
    const newSession: Session = { accessToken: response.accessToken, memberships: response.memberships }
    writeStoredSession(newSession)
    setSession(newSession)
  }, [])

  const logout = useCallback(async () => {
    if (session) {
      try {
        await logoutRequest(session.accessToken)
      } catch {
        // la sesión local se limpia de todas formas
      }
    }
    clearStoredSession()
    setSession(null)
  }, [session])

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated: session !== null,
        memberships: session?.memberships ?? [],
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
