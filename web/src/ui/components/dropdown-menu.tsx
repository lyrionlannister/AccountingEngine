import { useEffect, useRef, useState } from 'react'
import type { DropdownMenuProps } from '../props/dropdown-menu-props'

export const DropdownMenu = ({ trigger, items, align = 'right', ...rest }: DropdownMenuProps) => {
  const [open, setOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const onDocClick = (e: MouseEvent) => {
      if (!rootRef.current?.contains(e.target as Node)) setOpen(false)
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('click', onDocClick)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('click', onDocClick)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  return (
    <div ref={rootRef} className="relative inline-block">
      <span onClick={() => setOpen((v) => !v)}>{trigger}</span>
      {open && (
        <div
          role="menu"
          aria-label={rest['aria-label']}
          className={`absolute top-full z-10 mt-1 w-57.5 rounded-md border border-line bg-raised p-1 shadow-float ${
            align === 'right' ? 'right-0' : 'left-0'
          }`}
        >
          {items.map((item, i) =>
            item === 'divider' ? (
              <hr key={i} className="my-1 border-line" />
            ) : (
              <button
                key={item.label}
                type="button"
                role="menuitem"
                onClick={() => {
                  item.onSelect?.()
                  setOpen(false)
                }}
                className={`flex w-full items-center gap-2.5 rounded-sm px-2.5 py-2 text-left font-body text-sm hover:bg-muted/12 ${
                  item.danger ? 'text-error-text' : 'text-text'
                }`}
              >
                {item.icon}
                {item.label}
                {item.kbd && <kbd className="ml-auto font-mono text-[10.5px] text-faint">{item.kbd}</kbd>}
              </button>
            ),
          )}
        </div>
      )}
    </div>
  )
}
