import { postJson } from '../../lib/http'
import type { RegisterRequest, RegisterResponse } from '../types/register'

export const register = (request: RegisterRequest): Promise<RegisterResponse> =>
  postJson<RegisterResponse>('/users/register', request)
