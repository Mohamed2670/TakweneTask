import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Navigate, useLocation } from 'react-router'
import { ApiError } from '../api/http'
import { login } from '../auth/session'
import { useSession } from '../auth/useSession'
import { Brand } from '../components/Brand'
import { SessionScreen } from '../components/Feedback'

export function LoginPage() {
  const session = useSession()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [visible, setVisible] = useState(false)
  const [busy, setBusy] = useState(false)
  const [errors, setErrors] = useState<{ email?: string; password?: string; form?: string }>({})
  useEffect(() => { document.title = 'Sign in · Takwene' }, [])
  const requested = (location.state as { returnTo?: unknown } | null)?.returnTo
  const returnTo = typeof requested === 'string' && /^\/tracks(?:\/|\?|$)/.test(requested) ? requested : '/tracks'
  if (session.status === 'initializing') return <SessionScreen />
  if (session.status === 'unavailable') return <SessionScreen error={session.error} />
  if (session.status === 'authenticated') return <Navigate to={returnTo} replace />

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy) return
    const next: typeof errors = {}
    const normalized = email.trim()
    if (!normalized || normalized.length > 254 || !/^[^\s@]+@[^\s@]+$/.test(normalized)) next.email = 'Enter a valid email address.'
    if (!password || password.length > 256) next.password = 'Enter your password (up to 256 characters).'
    setErrors(next)
    if (Object.keys(next).length) return
    setBusy(true)
    try { await login(normalized, password) }
    catch (error) {
      if (error instanceof ApiError) {
        setErrors({
          email: error.fields.email?.[0], password: error.fields.password?.[0],
          form: error.status === 401 ? 'The email or password is incorrect.' : error.message,
        })
      } else setErrors({ form: 'We could not sign you in. Please try again.' })
    } finally { setBusy(false) }
  }
  return <main className="login-page">
    <section className="login-story" aria-label="Takwene">
      <Brand light />
      <div className="story-content"><span className="eyebrow">MADE FOR THE MUSIC</span>
        <h1>Every track.<br />A new beginning<span>.</span></h1>
        <p>A clear view of your music, your artists, and every platform along the way.</p>
        <div className="vinyl-scene" aria-hidden="true"><div className="scene-disc"><div className="scene-label">t<span>.</span></div></div><div className="scene-orbit" /><span className="scene-note">FROM MENA, TO EVERYWHERE</span></div>
      </div>
      <footer className="story-footer"><span>Content. Connected.</span><span>TAKWENE / MUSIC</span></footer>
    </section>
    <section className="login-form-section" aria-labelledby="sign-in-heading">
      <div className="mobile-brand"><Brand /></div>
      <div className="login-form-wrap"><span className="eyebrow">YOUR CATALOG STARTS HERE</span>
        <h2 id="sign-in-heading">Welcome back.</h2><p className="form-intro">Sign in to your track management workspace.</p>
        <form onSubmit={(event) => { void submit(event) }} noValidate aria-busy={busy}>
          <div className="form-field"><label htmlFor="email">Email address</label>
            <input id="email" name="email" type="email" autoComplete="username" placeholder="you@example.com" maxLength={254} value={email} disabled={busy}
              onChange={(event) => { setEmail(event.target.value); setErrors({}) }} aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? 'email-error' : undefined} />
            {errors.email && <p id="email-error" className="field-error">{errors.email}</p>}
          </div>
          <div className="form-field"><label htmlFor="password">Password</label><div className="password-field">
            <input id="password" name="password" type={visible ? 'text' : 'password'} autoComplete="current-password" placeholder="Enter your password" maxLength={256} value={password} disabled={busy}
              onChange={(event) => { setPassword(event.target.value); setErrors({}) }} aria-invalid={Boolean(errors.password)} aria-describedby={errors.password ? 'password-error' : undefined} />
            <button type="button" className="password-toggle" aria-label={visible ? 'Hide password' : 'Show password'} aria-pressed={visible} onClick={() => { setVisible((value) => !value) }} disabled={busy}>{visible ? 'Hide' : 'Show'}</button>
          </div>{errors.password && <p id="password-error" className="field-error">{errors.password}</p>}</div>
          {errors.form && <p className="form-error" role="alert">{errors.form}</p>}
          <button type="submit" className="button button-primary login-submit" disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}<span aria-hidden="true">↗</span></button>
        </form>
        <p className="login-help">Use your assigned staff account to access the catalog.</p>
      </div>
      <footer className="login-footer">Track management, with a little more clarity.</footer>
    </section>
  </main>
}
