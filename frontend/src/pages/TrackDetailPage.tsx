import { useCallback, useEffect } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { ApiError } from '../api/http'
import { getTrack } from '../api/tracks'
import { Artwork } from '../components/Artwork'
import { ErrorMessage, Loading } from '../components/Feedback'
import { StatusBadge } from '../components/StatusBadge'
import { useResource } from '../hooks/useResource'
import { formatRelease } from './TrackListPage'

function providerSymbol(name: string) {
  if (name === 'Spotify') return '≋'
  if (name === 'Apple Music') return '♪'
  return '▶'
}
export function TrackDetailPage() {
  const params = useParams()
  const location = useLocation()
  const id = Number(params.id)
  const load = useCallback((signal: AbortSignal) => {
    if (!Number.isSafeInteger(id) || id < 1 || id > 2147483647) return Promise.reject(new ApiError(400, 'This track link is not valid.'))
    return getTrack(id, signal)
  }, [id])
  const { data: track, loading, error, retry } = useResource(load)
  const requested = (location.state as { backTo?: unknown } | null)?.backTo
  const backTo = typeof requested === 'string' && /^\/tracks(?:\?|$)/.test(requested) ? requested : '/tracks'
  useEffect(() => { document.title = (track?.title ?? 'Track details') + ' · Takwene' }, [track?.title])
  return <>
    <Link className="back-link" to={backTo}><span aria-hidden="true">←</span>Back to library</Link>
    {loading ? <Loading message="Loading track details…" /> : error ? <div className="detail-error"><ErrorMessage message={error.message} retry={error instanceof ApiError && [400, 404].includes(error.status) ? undefined : retry} /></div> : track && <>
      <section className="track-hero" aria-labelledby="track-title"><Artwork id={track.id} large /><div className="track-hero-info"><span className="eyebrow">TRACK / {String(track.id).padStart(3, '0')}</span><h1 id="track-title">{track.title}</h1><p>{track.artistName}</p><StatusBadge status={track.status} /></div><span className="hero-mark" aria-hidden="true">↗</span></section>
      <section className="metadata-panel" aria-label="Release information"><dl><div><dt>Artist</dt><dd>{track.artistName}</dd></div><div><dt>Genre</dt><dd>{track.genre}</dd></div><div><dt>Release date</dt><dd>{formatRelease(track.releaseDate)}</dd></div><div><dt>ISRC</dt><dd className="isrc">{track.isrc}</dd></div></dl></section>
      <section className="distribution-section" aria-labelledby="distribution-heading"><div className="section-heading"><div><span className="eyebrow">FOLLOW THE RELEASE</span><h2 id="distribution-heading">Platform distribution</h2></div><span className="platform-count">{track.distributions.length} platform{track.distributions.length === 1 ? '' : 's'}</span></div>
        {track.distributions.length === 0 ? <div className="feedback empty-distributions"><span className="empty-record" aria-hidden="true">◎</span><h3>No platform submissions yet.</h3><p>This track has not been submitted to a digital service provider.</p></div> :
          <div className="distribution-grid">{track.distributions.map((distribution) => <article className="dsp-card" key={distribution.id}><div className="dsp-card-heading"><span className={'dsp-icon dsp-' + distribution.dspId} aria-hidden="true">{providerSymbol(distribution.dspName)}</span><StatusBadge status={distribution.status} /></div><h3>{distribution.dspName}</h3><span className="dsp-rule" /><p>Submitted</p><time dateTime={distribution.submittedAt}>{new Intl.DateTimeFormat('en', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(distribution.submittedAt))}</time></article>)}</div>}
        <p className="distribution-note">Platform statuses are tracked individually and may differ from the track’s overall status.</p>
      </section>
    </>}
  </>
}
