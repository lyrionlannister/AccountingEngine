import { EMPTY_STATE_SKELETON_ROWS } from '../constants/empty-state-skeleton'
import type { EmptyStateProps } from '../props/empty-state-props'

export const EmptyState = ({ icon, title, description, items, actions }: EmptyStateProps) => {
  return (
    <div className="grid grid-cols-1 gap-8 rounded-md border border-line bg-surface p-7 shadow-card md:grid-cols-[minmax(0,340px)_minmax(0,1fr)]">
      <div className="grid content-start gap-2.5">
        <span className="grid size-10 place-items-center rounded-md bg-primary/12 text-primary">{icon}</span>
        <h3 className="font-head text-base font-semibold text-text">{title}</h3>
        {description && <p className="text-[13px] text-muted">{description}</p>}
        {items && (
          <ul className="grid list-disc gap-1.5 pl-4 text-[13px] text-text marker:text-faint">
            {items.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        )}
        {actions && <div className="mt-1.5 flex flex-wrap gap-2">{actions}</div>}
      </div>
      <div className="overflow-hidden rounded border border-line" aria-hidden>
        {EMPTY_STATE_SKELETON_ROWS.map((widths, i) => (
          <div key={i} className={`grid grid-cols-[70px_110px_1fr_90px_100px] gap-4 px-4 py-3.5 ${i === 0 ? 'bg-bg' : 'border-t border-line'}`}>
            {widths.map((w, j) => (
              <span key={j} className={`h-2 rounded-sm bg-muted/18 ${i === 0 ? 'h-1.5 bg-muted/28' : ''}`} style={{ width: `${w}%` }} />
            ))}
            <span className="h-2 w-4/5 justify-self-end rounded-sm bg-muted/18" />
          </div>
        ))}
      </div>
    </div>
  )
}
