// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { App } from '../src/App'
import { ApiError } from '../src/api/http'
import { getTrack, listTracks } from '../src/api/tracks'
import { login } from '../src/auth/session'

const identity = vi.hoisted(() => ({
  state: { status: 'authenticated', user: { id: 1, name: 'Demo Operator', email: 'demo@example.com' }, error: null as string | null },
}))
vi.mock('../src/auth/useSession', () => ({ useSession: () => identity.state }))
vi.mock('../src/auth/session', () => ({ login: vi.fn(), logout: vi.fn(), retrySession: vi.fn() }))
vi.mock('../src/api/tracks', () => ({ getTrack: vi.fn(), listTracks: vi.fn() }))

const draft = { id: 1, title: 'Cairo Nights', artistId: 1, artistName: 'Layla Noor', releaseDate: '2026-09-01', genre: 'Arabic Pop', status: 'draft' as const }
const distributed = { ...draft, id: 3, title: 'Nile Breeze', genre: 'Electronic', status: 'distributed' as const }
const detail = {
  ...distributed, isrc: 'EGTKW2600003', distributions: [
    { id: 2, dspId: 1, dspName: 'Spotify', status: 'live' as const, submittedAt: '2026-08-20T12:00:00Z' },
    { id: 3, dspId: 2, dspName: 'Apple Music', status: 'pending' as const, submittedAt: '2026-08-20T12:00:00Z' },
  ],
}
function open(path = '/tracks') {
  return render(<MemoryRouter initialEntries={[path]}><App /></MemoryRouter>)
}
beforeEach(() => {
  vi.clearAllMocks()
  identity.state = { status: 'authenticated', user: { id: 1, name: 'Demo Operator', email: 'demo@example.com' }, error: null }
  vi.mocked(listTracks).mockImplementation(async (status) => status === 'distributed' ? [distributed] : [draft, distributed])
  vi.mocked(getTrack).mockResolvedValue(detail)
})
afterEach(cleanup)

describe('catalog views', () => {
  it('renders tracks, filters via the API, and preserves the filter through detail navigation', async () => {
    const user = userEvent.setup()
    open()
    await screen.findByRole('link', { name: 'Cairo Nights', exact: true })
    await user.selectOptions(screen.getByLabelText('Status'), 'distributed')
    await screen.findByText('1 track in this view')
    expect(vi.mocked(listTracks).mock.lastCall?.[0]).toBe('distributed')
    expect(screen.queryByRole('link', { name: 'Cairo Nights', exact: true })).toBeNull()
    await user.click(screen.getByRole('link', { name: 'Nile Breeze', exact: true }))
    await screen.findByRole('heading', { name: 'Nile Breeze', exact: true })
    expect(screen.getByText('EGTKW2600003')).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Spotify' })).toBeTruthy()
    expect(screen.getByText('Live', { exact: true })).toBeTruthy()
    expect(screen.getByText('Pending', { exact: true })).toBeTruthy()
    await user.click(screen.getByRole('link', { name: 'Back to library' }))
    await screen.findByText('1 track in this view')
    expect((screen.getByLabelText('Status') as HTMLSelectElement).value).toBe('distributed')
  })

  it('shows a clear empty-search state and can reset filters', async () => {
    const user = userEvent.setup()
    open()
    await screen.findByRole('link', { name: 'Cairo Nights', exact: true })
    await user.type(screen.getByLabelText('Search tracks or artists'), 'no such track')
    expect(screen.getByRole('heading', { name: 'No tracks match this view.' })).toBeTruthy()
    await user.click(screen.getByRole('button', { name: 'Clear filters' }))
    await screen.findByRole('link', { name: 'Cairo Nights', exact: true })
  })

  it('shows and recovers from a catalog request failure', async () => {
    vi.mocked(listTracks).mockRejectedValueOnce(new ApiError(0, 'Check your connection.'))
    const user = userEvent.setup()
    open()
    await screen.findByRole('alert')
    await user.click(screen.getByRole('button', { name: 'Try again' }))
    await screen.findByRole('link', { name: 'Cairo Nights', exact: true })
  })

  it('renders pending loading without pretending the catalog is empty', async () => {
    vi.mocked(listTracks).mockImplementationOnce(() => new Promise(() => undefined))
    open()
    await screen.findByText('Loading your catalog…')
    expect(screen.queryByText('No tracks match this view.')).toBeNull()
  })

  it('shows a meaningful not-found response without a misleading retry', async () => {
    vi.mocked(getTrack).mockRejectedValueOnce(new ApiError(404, 'Track not found.'))
    open('/tracks/2147483647')
    await screen.findByText('Track not found.')
    expect(screen.queryByRole('button', { name: 'Try again' })).toBeNull()
    expect(screen.getByRole('link', { name: 'Back to library' })).toBeTruthy()
  })

  it('rejects an invalid track link before making an API call', async () => {
    open('/tracks/not-an-id')
    await screen.findByText('This track link is not valid.')
    expect(getTrack).not.toHaveBeenCalled()
  })

  it('renders a track with no distributions correctly', async () => {
    vi.mocked(getTrack).mockResolvedValueOnce({ ...draft, isrc: 'EGTKW2600001', distributions: [] })
    open('/tracks/1')
    await screen.findByRole('heading', { name: 'No platform submissions yet.' })
  })
})

describe('login view', () => {
  it('validates missing fields without submitting credentials', async () => {
    identity.state.status = 'anonymous'
    const user = userEvent.setup()
    open('/login')
    await user.click(screen.getByRole('button', { name: 'Sign in', exact: true }))
    expect(screen.getByText('Enter a valid email address.')).toBeTruthy()
    expect(screen.getByText('Enter your password (up to 256 characters).')).toBeTruthy()
    expect(login).not.toHaveBeenCalled()
  })

  it('submits a normalized email and reports invalid credentials', async () => {
    identity.state.status = 'anonymous'
    vi.mocked(login).mockRejectedValueOnce(new ApiError(401, 'Invalid email or password.'))
    const user = userEvent.setup()
    open('/login')
    await user.type(screen.getByLabelText('Email address'), 'demo@example.com')
    await user.type(screen.getByLabelText('Password', { exact: true }), ' password ')
    await user.click(screen.getByRole('button', { name: 'Sign in', exact: true }))
    await waitFor(() => { expect(screen.getByRole('alert').textContent).toContain('The email or password is incorrect.') })
    expect(login).toHaveBeenCalledWith('demo@example.com', ' password ')
  })
})
