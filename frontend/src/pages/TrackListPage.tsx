import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router'
import { listTracks } from '../api/tracks'
import { Artwork } from '../components/Artwork'
import { ErrorMessage, Loading } from '../components/Feedback'
import { StatusBadge } from '../components/StatusBadge'
import { useResource } from '../hooks/useResource'
import type { TrackStatus } from '../types'

const statuses: TrackStatus[] = ['draft', 'submitted', 'distributed']
export function formatRelease(value: string) {
  return new Intl.DateTimeFormat('en', { dateStyle: 'medium', timeZone: 'UTC' }).format(new Date(value + 'T00:00:00Z'))
}
export function TrackListPage() {
  const [params, setParams] = useSearchParams()
  const location = useLocation()
  const selected = params.get('status') ?? ''
  const status = statuses.includes(selected as TrackStatus) ? selected as TrackStatus : ''
  const [search, setSearch] = useState('')
  const load = useCallback((signal: AbortSignal) => listTracks(status, signal), [status])
  const { data, loading, error, retry } = useResource(load)
  useEffect(() => { document.title = 'Track library · Takwene' }, [])
  const query = search.trim().toLocaleLowerCase()
  const shown = data?.filter((track) => !query || (track.title + ' ' + track.artistName).toLocaleLowerCase().includes(query)) ?? []
  return <>
    <div className="page-heading"><div><span className="eyebrow">YOUR MUSIC, IN VIEW</span><h1>Track library<span>.</span></h1><p>Every release, from the first draft to its next destination.</p></div>
      <button className="button button-secondary refresh-button" onClick={retry} disabled={loading}><span aria-hidden="true">↻</span>Refresh library</button>
    </div>
    <section className="catalog-panel" aria-label="Track catalog">
      <div className="catalog-toolbar"><div className="search-field"><span aria-hidden="true">⌕</span><label className="sr-only" htmlFor="track-search">Search tracks or artists</label><input id="track-search" type="search" maxLength={200} placeholder="Search tracks or artists" value={search} onChange={(event) => { setSearch(event.target.value) }} /></div>
        <div className="status-filter"><label htmlFor="status-filter">Status</label><select id="status-filter" value={status} onChange={(event) => { const next = new URLSearchParams(params); if (event.target.value) next.set('status', event.target.value); else next.delete('status'); setParams(next) }}>
          <option value="">All statuses</option><option value="draft">Draft</option><option value="submitted">Submitted</option><option value="distributed">Distributed</option>
        </select></div>
      </div>
      {loading ? <Loading /> : error ? <ErrorMessage message={error.message} retry={retry} /> : shown.length === 0 ? <div className="feedback empty-state"><span className="empty-record" aria-hidden="true">◎</span><h2>{query || status ? 'No tracks match this view.' : 'Your catalog is ready for its first track.'}</h2><p>{query || status ? 'Try a different search or status filter.' : 'Tracks will appear here once they are added.'}</p>{(query || status) && <button className="button button-secondary" onClick={() => { setSearch(''); setParams({}) }}>Clear filters</button>}</div> :
        <div className="table-scroll"><table className="track-table"><caption className="sr-only">Music tracks and their current status</caption><thead><tr><th scope="col">Track</th><th scope="col">Artist</th><th scope="col">Genre</th><th scope="col">Release date</th><th scope="col">Status</th><th scope="col"><span className="sr-only">Details</span></th></tr></thead><tbody>
          {shown.map((track) => <tr key={track.id}><td><div className="track-cell"><Artwork id={track.id} /><div><Link className="track-title" to={'/tracks/' + track.id} state={{ backTo: location.pathname + location.search }}>{track.title}</Link><span className="track-number">TKW / {String(track.id).padStart(3, '0')}</span></div></div></td><td className="artist-cell">{track.artistName}</td><td><span className="genre-chip">{track.genre}</span></td><td className="release-cell">{formatRelease(track.releaseDate)}</td><td><StatusBadge status={track.status} /></td><td><Link className="detail-arrow" to={'/tracks/' + track.id} state={{ backTo: location.pathname + location.search }} aria-label={'View details for ' + track.title}>↗</Link></td></tr>)}
        </tbody></table></div>}
      <div className="catalog-footer" aria-live="polite"><span>{loading ? 'Loading tracks…' : error ? 'Catalog unavailable' : String(shown.length) + ' track' + (shown.length === 1 ? '' : 's') + ' in this view'}</span><span>ONE CATALOG. EVERY DESTINATION.</span></div>
    </section>
    <div className="catalog-note"><span aria-hidden="true">↗</span><p>Select a track to see release information and its distribution across platforms.</p></div>
  </>
}
