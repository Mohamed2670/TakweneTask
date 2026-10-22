import { useState } from 'react'
import { Link, Navigate, Outlet, useLocation } from 'react-router'
import { logout } from '../auth/session'
import { useSession } from '../auth/useSession'
import { Brand } from './Brand'
import { SessionScreen } from './Feedback'

export function AppLayout() {
  const session = useSession()
  const location = useLocation()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  if (session.status === 'initializing') return <SessionScreen />
  if (session.status === 'unavailable') return <SessionScreen error={session.error} />
  if (session.status !== 'authenticated' || !session.user) return <Navigate to="/login" replace state={{ returnTo: location.pathname + location.search }} />
  const user = session.user
  async function signOut() {
    setBusy(true); setError(null)
    try { await logout() }
    catch { setError('We could not sign you out. Check your connection and try again.') }
    finally { setBusy(false) }
  }
  return <div className="app-shell">
    <a className="skip-link" href="#main-content">Skip to content</a>
    <aside className="sidebar"><Brand />
      <div className="sidebar-caption">WORKSPACE</div>
      <nav aria-label="Main navigation"><Link to="/tracks" className="nav-link" aria-current="page"><span className="nav-icon" aria-hidden="true">▤</span>Track library<span className="nav-arrow" aria-hidden="true">↗</span></Link></nav>
      <div className="sidebar-note"><span className="small-record" aria-hidden="true">◎</span><h2>Made for your next release.</h2><p>Your artists. Your catalog.<br />Every step in one place.</p><span className="note-line" /></div>
      <div className="account"><span className="avatar" aria-hidden="true">{user.name.slice(0, 1)}</span><div><strong>{user.name}</strong><span>Catalog workspace</span></div></div>
      <button className="sign-out" disabled={busy} onClick={() => { void signOut() }}>{busy ? 'Signing out…' : 'Sign out'}<span aria-hidden="true">↗</span></button>
    </aside>
    <div className="workspace"><header className="topbar"><span>WORKSPACE <span className="breadcrumb-slash">/</span> MUSIC CATALOG</span><span className="workspace-badge"><i aria-hidden="true" />MENA & beyond</span></header>
      <main id="main-content" className="main-content">{error && <p className="form-error" role="alert">{error}</p>}<Outlet /></main>
      <footer className="workspace-footer"><span>takwene. / track management</span><span>Made for the music.</span></footer>
    </div>
  </div>
}
