import type { DistributionStatus, TrackStatus } from '../types'

export function StatusBadge({ status }: { status: TrackStatus | DistributionStatus }) {
  return <span className={'status-badge status-' + status}><span aria-hidden="true" />{status.charAt(0).toUpperCase() + status.slice(1)}</span>
}
