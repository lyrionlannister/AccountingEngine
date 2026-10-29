import { SESSION_STORAGE_KEY } from '../constants/session-storage'
import type { Session } from '../types/auth'

export const readStoredSession = (): Session | null => {
  const raw = sessionStorage.getItem(SESSION_STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as Session
  } catch {
    return null
  }
}

export const writeStoredSession = (session: Session): void => {
  sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session))
}

export const clearStoredSession = (): void => {
  sessionStorage.removeItem(SESSION_STORAGE_KEY)
}
