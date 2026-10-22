export function Artwork({ id, large = false }: { id: number; large?: boolean }) {
  return <div aria-hidden="true" className={'artwork artwork-' + (id % 5) + (large ? ' artwork-large' : '')}>
    <div className="record"><div className="record-label"><span /></div></div>
  </div>
}
