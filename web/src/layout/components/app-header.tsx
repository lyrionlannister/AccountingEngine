import { CalendarDays, LogOut, Menu, Plus, Search, UserRound } from 'lucide-react'
import { useNavigate } from 'react-router'
import { Button } from '../../ui/components/button'
import { Badge } from '../../ui/components/badge'
import { DropdownMenu } from '../../ui/components/dropdown-menu'
import { useSidebar } from '../../ui/hooks/use-sidebar'
import { useAuth } from '../../auth/hooks/use-auth'

export const AppHeader = () => {
  const { toggleMobileSidebar } = useSidebar()
  const { logout } = useAuth()
  const navigate = useNavigate()

  const onLogout = async () => {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="flex items-center gap-3 border-b border-line bg-surface px-4.5 py-3 md:px-7">
      <Button size="md" iconOnly variant="ghost" aria-label="Abrir menú" className="md:hidden" onClick={toggleMobileSidebar}>
        <Menu className="size-4.5" aria-hidden />
      </Button>

      <div className="hidden max-w-90 flex-1 items-center gap-2 rounded border border-line bg-bg px-2.5 py-1.75 text-muted md:flex">
        <Search className="size-3.5" aria-hidden />
        <span className="text-sm">Buscar póliza, cuenta o RFC</span>
        <kbd className="ml-auto rounded border border-line px-1.5 py-0.5 font-mono text-[10.5px]">⌘K</kbd>
      </div>

      <div className="ml-auto flex items-center gap-2">
        <span className="hidden lg:inline-flex">
          <Badge tone="success">Periodo abierto</Badge>
        </span>
        <span className="hidden md:inline-flex">
          <Button size="md" variant="secondary">
            <CalendarDays className="size-3.5" aria-hidden />
            Octubre 2026
          </Button>
        </span>
        <Button size="md" iconOnly variant="primary" aria-label="Nueva póliza" className="md:hidden">
          <Plus className="size-4" aria-hidden />
        </Button>
        <span className="hidden md:inline-flex">
          <Button size="md" variant="primary">
            <Plus className="size-4" aria-hidden />
            Nueva póliza
          </Button>
        </span>

        <DropdownMenu
          aria-label="Menú de usuario"
          trigger={
            <Button size="md" iconOnly variant="ghost" aria-label="Menú de usuario">
              <UserRound className="size-4.5" aria-hidden />
            </Button>
          }
          items={[{ label: 'Cerrar sesión', icon: <LogOut className="size-4" aria-hidden />, danger: true, onSelect: onLogout }]}
        />
      </div>
    </header>
  )
}
