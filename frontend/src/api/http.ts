const configuredUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:7270/api'
const parsedUrl = new URL(configuredUrl)
if (parsedUrl.protocol !== 'https:' && !['localhost', '127.0.0.1'].includes(parsedUrl.hostname)) {
  throw new Error('The API must use HTTPS.')
}
export const API_URL = configuredUrl.replace(/\/+$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly fields: Record<string, string[]>
  constructor(status: number, message: string, fields: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fields = fields
  }
}
export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PATCH'
  body?: unknown
  signal?: AbortSignal
  token?: string
  authCookie?: boolean
}

export async function http<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers = new Headers({ Accept: 'application/json' })
  if (options.body !== undefined) headers.set('Content-Type', 'application/json')
  if (options.token) headers.set('Authorization', 'Bearer ' + options.token)
  if (options.authCookie) headers.set('X-CSRF', '1')
  const timeout = AbortSignal.timeout(15000)
  let response: Response
  try {
    response = await fetch(API_URL + path, {
      method: options.method ?? 'GET',
      headers,
      credentials: options.authCookie ? 'include' : 'omit',
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      signal: options.signal ? AbortSignal.any([options.signal, timeout]) : timeout,
      cache: 'no-store',
    })
  } catch (error) {
    if (options.signal?.aborted) throw error
    throw new ApiError(0, 'We could not reach the server. Check your connection and try again.')
  }
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { title?: unknown; errors?: unknown } | null
    const fields: Record<string, string[]> = {}
    if (problem?.errors && typeof problem.errors === 'object') {
      for (const [field, messages] of Object.entries(problem.errors)) {
        if (Array.isArray(messages)) fields[field.toLowerCase()] = messages.filter((value): value is string => typeof value === 'string')
      }
    }
    const message = response.status >= 500
      ? 'The server is temporarily unavailable. Please try again.'
      : typeof problem?.title === 'string' ? problem.title : 'The request could not be completed.'
    throw new ApiError(response.status, message, fields)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
