import { useEffect, useSyncExternalStore } from 'react'
import { getSessionSnapshot, initializeSession, subscribeSession } from './session'

export function useSession() {
  const session = useSyncExternalStore(subscribeSession, getSessionSnapshot)
  useEffect(() => { void initializeSession() }, [])
  return session
}
