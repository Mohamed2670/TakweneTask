import type { TrackDetail, TrackStatus, TrackSummary } from '../types'
import { authenticatedRequest } from './client'

export function listTracks(status: TrackStatus | '', signal?: AbortSignal) {
  const query = status ? '?status=' + encodeURIComponent(status) : ''
  return authenticatedRequest<TrackSummary[]>('/tracks' + query, { signal })
}
export function getTrack(id: number, signal?: AbortSignal) {
  return authenticatedRequest<TrackDetail>('/tracks/' + id, { signal })
}
