import { invalidateSession, refreshSession } from '../auth/session'
import { tokenStore } from '../auth/tokenStore'
import { ApiError, http } from './http'
import type { RequestOptions } from './http'

export async function authenticatedRequest<T>(path: string, options: Omit<RequestOptions, 'token' | 'authCookie'> = {}): Promise<T> {
  let access = tokenStore.get()
  if (!access || access.expiresAt <= Date.now()) {
    await refreshSession()
    access = tokenStore.get()
  }
  const usedToken = access?.token
  try {
    return await http<T>(path, { ...options, token: usedToken })
  } catch (error) {
    if (!(error instanceof ApiError) || error.status !== 401) throw error
    if (options.signal?.aborted) throw error
    const newer = tokenStore.get()
    if (!newer || newer.token === usedToken || newer.expiresAt <= Date.now()) await refreshSession()
    try {
      return await http<T>(path, { ...options, token: tokenStore.get()?.token })
    } catch (retryError) {
      if (retryError instanceof ApiError && retryError.status === 401) invalidateSession()
      throw retryError
    }
  }
}
