export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

type ProblemDetails = {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export const postJson = async <TResponse>(path: string, body?: unknown, accessToken?: string): Promise<TResponse> => {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (accessToken) headers.Authorization = `Bearer ${accessToken}`

  const response = await fetch(`/api${path}`, {
    method: 'POST',
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (response.status === 204) return undefined as TResponse

  const text = await response.text()
  const data = text ? JSON.parse(text) : undefined

  if (!response.ok) {
    const problem = data as ProblemDetails | undefined
    throw new ApiError(
      response.status,
      problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado. Intenta de nuevo.',
      problem?.errors ?? {},
    )
  }

  return data as TResponse
}

export const firstFieldError = (fieldErrors: Record<string, string[]>, field: string): string | undefined => {
  const key = Object.keys(fieldErrors).find((k) => k.toLowerCase() === field.toLowerCase())
  return key ? fieldErrors[key][0] : undefined
}
