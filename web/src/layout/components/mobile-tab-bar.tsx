import { Layers } from 'lucide-react'
import { Link, useLocation } from 'react-router'
import { useSidebar } from '../../ui/hooks/use-sidebar'
import { NAV_ITEMS } from '../constants/nav-items'
import { isNavPathActive } from '../utils/is-nav-path-active'

export const MobileTabBar = () => {
  const { pathname } = useLocation()
  const { toggleMobileSidebar } = useSidebar()
  const primary = NAV_ITEMS.slice(0, 4)

  return (
    <nav aria-label="Navegación principal" className="grid grid-cols-5 gap-1 border-t border-line bg-surface px-1 pt-1.5 pb-3.5 md:hidden">
      {primary.map((item) => {
        const Icon = item.icon
        const active = isNavPathActive(pathname, item.to)
        return (
          <Link
            key={item.key}
            to={item.to}
            className={`grid justify-items-center gap-0.75 rounded py-1.5 text-[10.5px] font-medium no-underline ${
              active ? 'text-primary' : 'text-muted'
            }`}
          >
            <Icon className="size-5" aria-hidden />
            <span>{item.label}</span>
          </Link>
        )
      })}
      <button type="button" onClick={toggleMobileSidebar} className="grid justify-items-center gap-0.75 rounded py-1.5 text-[10.5px] font-medium text-muted">
        <Layers className="size-5" aria-hidden />
        <span>Más</span>
      </button>
    </nav>
  )
}
