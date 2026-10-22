import { Component } from 'react'
import type { ReactNode } from 'react'
import { Link, Navigate, Route, Routes } from 'react-router'
import { AppLayout } from './components/AppLayout'
import { Brand } from './components/Brand'
import { LoginPage } from './pages/LoginPage'
import { TrackDetailPage } from './pages/TrackDetailPage'
import { TrackListPage } from './pages/TrackListPage'

class AppErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }
  static getDerivedStateFromError() { return { failed: true } }
  render() {
    if (this.state.failed) return <main className="session-screen"><Brand /><div className="feedback"><h1>Let’s try that again.</h1><p>The workspace could not be displayed.</p><button className="button button-primary" onClick={() => { window.location.reload() }}>Reload workspace</button></div></main>
    return this.props.children
  }
}
export function App() {
  return <AppErrorBoundary><Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route element={<AppLayout />}>
      <Route path="/tracks" element={<TrackListPage />} />
      <Route path="/tracks/:id" element={<TrackDetailPage />} />
      <Route path="/" element={<Navigate to="/tracks" replace />} />
      <Route path="*" element={<div className="feedback"><h1>This page isn’t in the catalog.</h1><Link className="button button-primary" to="/tracks">Back to library</Link></div>} />
    </Route>
  </Routes></AppErrorBoundary>
}
