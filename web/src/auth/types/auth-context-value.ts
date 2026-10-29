import type { LoginRequest, MembershipSummary } from './auth'

export type AuthContextValue = {
  isAuthenticated: boolean
  memberships: MembershipSummary[]
  login: (request: LoginRequest) => Promise<void>
  logout: () => Promise<void>
}
