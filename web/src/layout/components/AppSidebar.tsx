import { Link, useLocation } from 'react-router'
import { useSidebar } from '../../ui/hooks/use-sidebar'
import { Logo } from '../../ui/components/Logo'
import { NAV_GROUP_START_KEYS, NAV_ITEMS } from '../constants/nav-items'
import { isNavPathActive } from '../utils/is-nav-path-active'
import type { AppSidebarProps } from '../props/app-sidebar-props'

export const AppSidebar = ({ variant = 'rail' }: AppSidebarProps) => {
  const { isExpanded, isHovered, setIsHovered, closeMobileSidebar } = useSidebar()
  const { pathname } = useLocation()
  const expanded = variant === 'drawer' || isExpanded || isHovered

  return (
    <aside
      onMouseEnter={() => variant === 'rail' && setIsHovered(true)}
      onMouseLeave={() => variant === 'rail' && setIsHovered(false)}
      className={`flex h-full flex-col gap-5 overflow-y-auto border-r border-side-line bg-side px-3 py-4.5 text-side-text transition-[width] duration-200 ${
        expanded ? 'w-59' : 'w-18'
      }`}
    >
      <div className={`flex items-center gap-2.5 px-2 font-head text-[15px] ${expanded ? '' : 'justify-center'}`}>
        <Logo className="size-5.5 text-side-mark" />
        {expanded && <b className="font-bold">EsQuadra</b>}
      </div>

      <button
        type="button"
        className={`grid gap-0.5 rounded border border-side-line px-2.5 py-2.5 text-left ${expanded ? '' : 'justify-items-center'}`}
      >
        {expanded ? (
          <>
            <b className="text-[13px] font-semibold">Mi empresa</b>
            <small className="font-mono text-[11px] text-side-muted">Ejercicio 2026</small>
          </>
        ) : (
          <b className="text-[11px] font-semibold">ME</b>
        )}
      </button>

      <nav aria-label="Módulos" className="grid gap-0.5">
        {NAV_ITEMS.map((item) => {
          const Icon = item.icon
          const active = isNavPathActive(pathname, item.to)
          return (
            <div key={item.key}>
              {NAV_GROUP_START_KEYS.has(item.key) && expanded && (
                <div className={`px-2.5 pt-3.5 pb-1.5 font-mono text-[10.5px] tracking-wider text-side-muted uppercase ${item.zone ? 'text-zone-text' : ''}`}>
                  {item.group}
                </div>
              )}
              <Link
                to={item.to}
                onClick={closeMobileSidebar}
                title={item.label}
                aria-current={active ? 'page' : undefined}
                className={`flex items-center gap-2.5 rounded px-2.5 py-1.75 text-[13px] no-underline ${
                  expanded ? '' : 'justify-center'
                } ${active ? 'bg-side-active font-medium text-side-active-text' : 'text-side-text hover:bg-side-hover'}`}
              >
                <Icon className="size-4 shrink-0" aria-hidden />
                {expanded && <span>{item.label}</span>}
              </Link>
            </div>
          )
        })}
      </nav>

      <div className={`mt-auto flex items-center gap-2.5 border-t border-side-line pt-2.5 ${expanded ? '' : 'justify-center'}`}>
        <span className="grid size-7 shrink-0 place-items-center rounded bg-side-hover font-head text-[11px] font-semibold">MR</span>
        {expanded && (
          <div className="grid leading-tight">
            <span>Mariana Ríos</span>
            <small className="text-[11px] text-side-muted">Contadora general</small>
          </div>
        )}
      </div>
    </aside>
  )
}
