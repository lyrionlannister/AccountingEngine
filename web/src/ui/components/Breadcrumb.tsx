import { ChevronRight } from 'lucide-react'
import type { BreadcrumbProps } from '../props/breadcrumb-props'

export const Breadcrumb = ({ items }: BreadcrumbProps) => {
  return (
    <div className="flex items-center gap-1.5 font-body text-[12.5px] text-muted">
      {items.map((item, i) => {
        const isLast = i === items.length - 1
        return (
          <span key={item.label} className="flex items-center gap-1.5">
            {i > 0 && <ChevronRight className="size-3 text-faint" aria-hidden />}
            {isLast || !item.href ? (
              <b className="font-medium text-text">{item.label}</b>
            ) : (
              <a href={item.href} className="text-link">
                {item.label}
              </a>
            )}
          </span>
        )
      })}
    </div>
  )
}
