import { ApiError, http } from '../api/http'
import type { AuthResponse, User } from '../types'
import { tokenStore } from './tokenStore'

export interface SessionState {
  status: 'initializing' | 'authenticated' | 'anonymous' | 'unavailable'
  user: User | null
  error: string | null
}
let state: SessionState = { status: 'initializing', user: null, error: null }
const listeners = new Set<() => void>()
let refreshing: Promise<AuthResponse> | null = null
let initialization: Promise<void> | null = null

const channel = typeof window !== 'undefined' && typeof BroadcastChannel !== 'undefined'
  ? new BroadcastChannel('takwene-auth-events') : null
function publish(next: SessionState) {
  state = next
  for (const listener of listeners) listener()
}
export function subscribeSession(listener: () => void) {
  listeners.add(listener)
  return () => { listeners.delete(listener) }
}
export function getSessionSnapshot() { return state }
export function invalidateSession(broadcast = false) {
  tokenStore.clear()
  publish({ status: 'anonymous', user: null, error: null })
  if (broadcast) channel?.postMessage({ type: 'signed-out' })
}
function acceptSession(data: AuthResponse) {
  const expiresAt = Date.parse(data.expiresAt)
  if (!data.accessToken || !Number.isFinite(expiresAt) || expiresAt <= Date.now()
      || !data.user?.id || !data.user.name || !data.user.email) {
    throw new ApiError(502, 'Your session could not be restored. Please try again.')
  }
  tokenStore.set({ token: data.accessToken, expiresAt })
  publish({ status: 'authenticated', user: data.user, error: null })
}
async function withSessionLock<T>(action: () => Promise<T>): Promise<T> {
  if (!globalThis.navigator?.locks) {
    throw new ApiError(0, 'Please use an up-to-date browser to sign in securely.')
  }
  return navigator.locks.request('takwene-auth-session', { signal: AbortSignal.timeout(20000) }, action)
}
export function refreshSession(): Promise<AuthResponse> {
  if (refreshing) return refreshing
  refreshing = withSessionLock(async () => {
    try {
      const data = await http<AuthResponse>('/auth/refresh', { method: 'POST', authCookie: true })
      acceptSession(data)
      return data
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) invalidateSession(true)
      else publish({
        status: 'unavailable', user: null,
        error: error instanceof Error ? error.message : 'Your session could not be restored.',
      })
      throw error
    }
  }).catch((error: unknown) => {
    if (state.status === 'initializing') {
      publish({ status: 'unavailable', user: null, error: error instanceof Error ? error.message : 'Your session could not be restored.' })
    }
    throw error
  }).finally(() => { refreshing = null })
  return refreshing
}
export function initializeSession(): Promise<void> {
  initialization ??= refreshSession().then(() => undefined).catch(() => undefined)
  return initialization
}
export async function retrySession(): Promise<void> {
  publish({ status: 'initializing', user: null, error: null })
  await refreshSession().catch(() => undefined)
}
export async function login(email: string, password: string): Promise<void> {
  await withSessionLock(async () => {
    const data = await http<AuthResponse>('/auth/login', {
      method: 'POST', body: { email: email.trim(), password }, authCookie: true,
    })
    acceptSession(data)
    channel?.postMessage({ type: 'signed-in' })
  })
}
export async function logout(): Promise<void> {
  await withSessionLock(async () => {
    await http<void>('/auth/logout', { method: 'POST', authCookie: true })
    invalidateSession(true)
  })
}
if (channel) {
  channel.onmessage = (event: MessageEvent<unknown>) => {
    const message = event.data as { type?: unknown } | null
    if (message?.type === 'signed-out') invalidateSession()
    if (message?.type === 'signed-in') {
      tokenStore.clear()
      publish({ status: 'initializing', user: null, error: null })
      void refreshSession().catch(() => undefined)
    }
  }
}
