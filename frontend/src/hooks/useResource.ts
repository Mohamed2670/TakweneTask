import { useEffect, useState } from 'react'

interface Resource<T> { data: T | null; loading: boolean; error: Error | null }
export function useResource<T>(loader: (signal: AbortSignal) => Promise<T>) {
  const [attempt, setAttempt] = useState(0)
  const [resource, setResource] = useState<Resource<T>>({ data: null, loading: true, error: null })
  useEffect(() => {
    const controller = new AbortController()
    setResource({ data: null, loading: true, error: null })
    void loader(controller.signal).then(
      (data) => { if (!controller.signal.aborted) setResource({ data, loading: false, error: null }) },
      (error: unknown) => {
        if (!controller.signal.aborted) setResource({ data: null, loading: false, error: error instanceof Error ? error : new Error('Please try again.') })
      },
    )
    return () => { controller.abort() }
  }, [loader, attempt])
  return { ...resource, retry: () => { setAttempt((value) => value + 1) } }
}
