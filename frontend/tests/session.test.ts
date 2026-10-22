import { beforeEach, describe, expect, it, vi } from 'vitest'

const fetchMock = vi.fn()
let queue = Promise.resolve<unknown>(undefined)
const lock = vi.fn((_name: string, _options: unknown, action: () => Promise<unknown>) => {
  const result = queue.then(action)
  queue = result.catch(() => undefined)
  return result
})
const user = { id: 1, name: 'Demo Operator', email: 'demo@example.com' }
const sessionData = (token = 'test-access-token') => ({
  accessToken: token, expiresAt: new Date(Date.now() + 600000).toISOString(), user,
})
const json = (data: unknown, status = 200) => new Response(JSON.stringify(data), {
  status, headers: { 'Content-Type': 'application/json' },
})
const unauthorized = () => json({ title: 'Unauthorized' }, 401)

beforeEach(() => {
  vi.resetModules()
  fetchMock.mockReset()
  lock.mockClear()
  queue = Promise.resolve()
  vi.stubGlobal('fetch', fetchMock)
  vi.stubGlobal('navigator', { locks: { request: lock } })
})

describe('memory token storage', () => {
  it('keeps get/set/clear behind a replaceable adapter', async () => {
    const { createMemoryTokenStore } = await import('../src/auth/tokenStore')
    const first = createMemoryTokenStore()
    const second = createMemoryTokenStore()
    first.set({ token: 'fake-token', expiresAt: 100 })
    expect(first.get()?.token).toBe('fake-token')
    expect(second.get()).toBeNull()
    first.clear()
    expect(first.get()).toBeNull()
  })
})

describe('session lifecycle', () => {
  it('deduplicates initialization and uses credentialed CSRF-protected refresh', async () => {
    fetchMock.mockResolvedValue(json(sessionData()))
    const session = await import('../src/auth/session')
    const { tokenStore } = await import('../src/auth/tokenStore')
    await Promise.all([session.initializeSession(), session.initializeSession(), session.initializeSession()])
    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(lock).toHaveBeenCalledTimes(1)
    const [url, options] = fetchMock.mock.calls[0]
    expect(url).toBe('https://localhost:7270/api/auth/refresh')
    expect(options.credentials).toBe('include')
    expect(options.headers.get('X-CSRF')).toBe('1')
    expect(options.headers.has('Authorization')).toBe(false)
    expect(session.getSessionSnapshot().status).toBe('authenticated')
    expect(tokenStore.get()?.token).toBe('test-access-token')
  })

  it('treats rejected refresh as anonymous instead of a transport error', async () => {
    fetchMock.mockResolvedValue(unauthorized())
    const session = await import('../src/auth/session')
    await session.initializeSession()
    expect(session.getSessionSnapshot()).toEqual({ status: 'anonymous', user: null, error: null })
  })

  it('keeps server failures distinct and supports session retry', async () => {
    fetchMock.mockRejectedValueOnce(new TypeError('network')).mockResolvedValueOnce(json(sessionData()))
    const session = await import('../src/auth/session')
    await session.initializeSession()
    expect(session.getSessionSnapshot().status).toBe('unavailable')
    await session.retrySession()
    expect(session.getSessionSnapshot().status).toBe('authenticated')
  })

  it('trims email but never trims a password', async () => {
    fetchMock.mockResolvedValue(json(sessionData()))
    const session = await import('../src/auth/session')
    await session.login('  demo@example.com  ', ' password with spaces ')
    expect(JSON.parse(fetchMock.mock.calls[0][1].body)).toEqual({
      email: 'demo@example.com', password: ' password with spaces ',
    })
  })

  it('rejects malformed or expired session responses', async () => {
    fetchMock.mockResolvedValue(json({ ...sessionData(), expiresAt: '2000-01-01T00:00:00Z' }))
    const session = await import('../src/auth/session')
    const { tokenStore } = await import('../src/auth/tokenStore')
    await session.initializeSession()
    expect(session.getSessionSnapshot().status).toBe('unavailable')
    expect(tokenStore.get()).toBeNull()
  })

  it('requires cross-tab locking rather than silently allowing unsafe rotations', async () => {
    vi.stubGlobal('navigator', {})
    const session = await import('../src/auth/session')
    await session.initializeSession()
    expect(session.getSessionSnapshot().status).toBe('unavailable')
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('clears in-memory identity only after successful server logout', async () => {
    fetchMock.mockResolvedValueOnce(json(sessionData())).mockResolvedValueOnce(new Response(null, { status: 204 }))
    const session = await import('../src/auth/session')
    const { tokenStore } = await import('../src/auth/tokenStore')
    await session.login(user.email, 'password')
    await session.logout()
    expect(session.getSessionSnapshot().status).toBe('anonymous')
    expect(tokenStore.get()).toBeNull()
  })

  it('does not claim logout succeeded when the server was unreachable', async () => {
    fetchMock.mockResolvedValueOnce(json(sessionData())).mockRejectedValueOnce(new TypeError('network'))
    const session = await import('../src/auth/session')
    await session.login(user.email, 'password')
    await expect(session.logout()).rejects.toMatchObject({ status: 0 })
    expect(session.getSessionSnapshot().status).toBe('authenticated')
  })
})

describe('authenticated API requests', () => {
  it('shares refresh for simultaneous failed requests and retries with the new token', async () => {
    fetchMock.mockResolvedValueOnce(unauthorized()).mockResolvedValueOnce(unauthorized())
      .mockResolvedValueOnce(json(sessionData('new-token')))
      .mockResolvedValueOnce(json({ id: 1 })).mockResolvedValueOnce(json({ id: 2 }))
    const { tokenStore } = await import('../src/auth/tokenStore')
    tokenStore.set({ token: 'old-token', expiresAt: Date.now() + 60000 })
    const { authenticatedRequest } = await import('../src/api/client')
    const results = await Promise.all([authenticatedRequest('/auth/me'), authenticatedRequest('/auth/me')])
    expect(results).toHaveLength(2)
    expect(fetchMock.mock.calls.filter(([url]) => url.endsWith('/auth/refresh'))).toHaveLength(1)
    expect(fetchMock.mock.calls.slice(3).every(([, options]) => options.headers.get('Authorization') === 'Bearer new-token')).toBe(true)
  })

  it('does not refresh or retry indefinitely on a second 401', async () => {
    fetchMock.mockResolvedValueOnce(unauthorized()).mockResolvedValueOnce(json(sessionData('new-token')))
      .mockResolvedValueOnce(unauthorized())
    const { tokenStore } = await import('../src/auth/tokenStore')
    tokenStore.set({ token: 'old-token', expiresAt: Date.now() + 60000 })
    const session = await import('../src/auth/session')
    const { authenticatedRequest } = await import('../src/api/client')
    await expect(authenticatedRequest('/auth/me')).rejects.toMatchObject({ status: 401 })
    expect(fetchMock).toHaveBeenCalledTimes(3)
    expect(tokenStore.get()).toBeNull()
    expect(session.getSessionSnapshot().status).toBe('anonymous')
  })

  it('refreshes expired memory tokens before sending a protected request', async () => {
    fetchMock.mockResolvedValueOnce(json(sessionData('renewed-token'))).mockResolvedValueOnce(json({ ok: true }))
    const { tokenStore } = await import('../src/auth/tokenStore')
    tokenStore.set({ token: 'expired', expiresAt: Date.now() - 1000 })
    const { authenticatedRequest } = await import('../src/api/client')
    await authenticatedRequest('/auth/me')
    expect(fetchMock.mock.calls[0][0]).toContain('/auth/refresh')
    expect(fetchMock.mock.calls[1][1].headers.get('Authorization')).toBe('Bearer renewed-token')
  })

  it('does not treat forbidden responses as an expired token', async () => {
    fetchMock.mockResolvedValue(json({ title: 'Forbidden' }, 403))
    const { tokenStore } = await import('../src/auth/tokenStore')
    tokenStore.set({ token: 'valid', expiresAt: Date.now() + 60000 })
    const { authenticatedRequest } = await import('../src/api/client')
    await expect(authenticatedRequest('/auth/me')).rejects.toMatchObject({ status: 403 })
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })
})
