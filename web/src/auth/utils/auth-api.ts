import { postJson } from '../../lib/http'
import type { LoginRequest, LoginResponse } from '../types/auth'

export const login = (request: LoginRequest): Promise<LoginResponse> => postJson<LoginResponse>('/auth/login', request)

export const logout = (accessToken: string): Promise<void> => postJson<void>('/auth/logout', undefined, accessToken)
