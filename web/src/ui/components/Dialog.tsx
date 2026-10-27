import { useEffect, useId, useRef } from 'react'
import { X } from 'lucide-react'
import { Button } from './Button'
import type { DialogProps } from '../props/dialog-props'

export const Dialog = ({ open, onClose, title, description, children, footer }: DialogProps) => {
  const titleId = useId()
  const bodyRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', onKey)
    const firstField = bodyRef.current?.querySelector<HTMLElement>('input, textarea, select, button')
    firstField?.focus({ preventScroll: true })
    return () => document.removeEventListener('keydown', onKey)
  }, [open, onClose])

  if (!open) return null

  return (
    <div
      className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-scrim pt-18 pb-8"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose()
      }}
    >
      <div role="dialog" aria-modal="true" aria-labelledby={titleId} className="grid w-190 max-w-[calc(100%-48px)] rounded-xl border border-line bg-raised shadow-float">
        <div className="flex items-start justify-between gap-3 border-b border-line px-5 py-4">
          <div>
            <h2 id={titleId} className="font-head text-[17px] font-semibold text-text">
              {title}
            </h2>
            {description && <p className="mt-1 text-[12.5px] text-muted">{description}</p>}
          </div>
          <Button size="sm" iconOnly variant="ghost" aria-label="Cerrar" onClick={onClose}>
            <X className="size-4" aria-hidden />
          </Button>
        </div>
        <div ref={bodyRef} className="grid gap-3.5 px-5 py-4">
          {children}
        </div>
        {footer && <div className="flex items-center justify-between gap-3 rounded-b-xl border-t border-line bg-bg px-5 py-3.5">{footer}</div>}
      </div>
    </div>
  )
}
