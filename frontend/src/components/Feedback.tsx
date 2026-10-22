import { Brand } from './Brand'
import { retrySession } from '../auth/session'

export function Loading({ message = 'Loading your catalog…' }: { message?: string }) {
  return <div className="feedback" role="status"><span className="spinner" aria-hidden="true" /><p>{message}</p></div>
}
export function ErrorMessage({ message, retry }: { message: string; retry?: () => void }) {
  return <div className="feedback feedback-error" role="alert">
    <span className="feedback-icon" aria-hidden="true">!</span><h2>Something went wrong</h2><p>{message}</p>
    {retry && <button className="button button-secondary" onClick={retry}>Try again</button>}
  </div>
}
export function SessionScreen({ error }: { error?: string | null }) {
  return <main className="session-screen"><Brand />
    {error ? <ErrorMessage message={error} retry={() => { void retrySession() }} /> : <Loading message="Restoring your session…" />}
  </main>
}
