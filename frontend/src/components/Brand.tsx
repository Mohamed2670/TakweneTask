import { Link } from 'react-router'

export function Brand({ light = false }: { light?: boolean }) {
  return <Link to="/tracks" className={'brand' + (light ? ' brand-light' : '')} aria-label="Takwene home">
    <span className="brand-symbol" aria-hidden="true"><i /><i /><i /><i /></span>
    <span>takwene<span className="brand-period">.</span></span>
  </Link>
}
