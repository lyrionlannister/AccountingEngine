import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from './Button'
import type { PaginationProps } from '../props/pagination-props'

export const Pagination = ({ page, pageCount, totalLabel, onChange }: PaginationProps) => {
  const pages = Array.from({ length: pageCount }, (_, i) => i + 1).filter(
    (p) => p === 1 || p === pageCount || Math.abs(p - page) <= 1,
  )

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 font-body text-xs text-muted">
      {totalLabel && <span>{totalLabel}</span>}
      <div className="flex gap-1">
        <Button size="sm" iconOnly aria-label="Anterior" disabled={page <= 1} onClick={() => onChange(page - 1)}>
          <ChevronLeft className="size-3.5" aria-hidden />
        </Button>
        {pages.map((p, i) => (
          <span key={p} className="flex items-center">
            {i > 0 && pages[i - 1] !== p - 1 && <span className="px-1">…</span>}
            <button
              type="button"
              onClick={() => onChange(p)}
              className={
                'min-w-7 rounded border px-2 py-1 font-body text-xs font-medium ' +
                (p === page ? 'border-text bg-text text-bg' : 'border-line bg-surface text-text hover:bg-bg')
              }
            >
              {p}
            </button>
          </span>
        ))}
        <Button size="sm" iconOnly aria-label="Siguiente" disabled={page >= pageCount} onClick={() => onChange(page + 1)}>
          <ChevronRight className="size-3.5" aria-hidden />
        </Button>
      </div>
    </div>
  )
}
