export type TrackStatus = 'draft' | 'submitted' | 'distributed'
export type DistributionStatus = 'pending' | 'live' | 'rejected'

export interface User { id: number; name: string; email: string }
export interface AuthResponse { accessToken: string; expiresAt: string; user: User }
export interface TrackSummary {
  id: number; title: string; artistId: number; artistName: string
  releaseDate: string; genre: string; status: TrackStatus
}
export interface Distribution {
  id: number; dspId: number; dspName: string; submittedAt: string; status: DistributionStatus
}
export interface TrackDetail extends TrackSummary { isrc: string; distributions: Distribution[] }
