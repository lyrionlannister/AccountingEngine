import { useEffect } from 'react'
import { Outlet } from 'react-router'
import { SidebarProvider } from '../../ui/components/sidebar-provider'
import { useSidebar } from '../../ui/hooks/use-sidebar'
import { AppHeader } from './app-header'
import { AppSidebar } from './app-sidebar'
import { MobileTabBar } from './mobile-tab-bar'

const LayoutContent = () => {
  const { isMobileOpen, closeMobileSidebar } = useSidebar()

  useEffect(() => {
    if (!isMobileOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeMobileSidebar()
    }
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [isMobileOpen, closeMobileSidebar])

  return (
    <div className="flex h-screen overflow-hidden bg-bg">
      <div className="hidden md:block">
        <AppSidebar />
      </div>

      {isMobileOpen && (
        <div className="fixed inset-0 z-40 flex md:hidden">
          <div className="absolute inset-0 bg-scrim" onClick={closeMobileSidebar} aria-hidden />
          <div className="relative">
            <AppSidebar variant="drawer" />
          </div>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col overflow-hidden">
        <AppHeader />
        <div className="min-h-0 flex-1 overflow-y-auto">
          <div className="grid min-h-full p-4.5 md:p-7">
            <Outlet />
          </div>
        </div>
        <MobileTabBar />
      </div>
    </div>
  )
}

export const AppLayout = () => {
  return (
    <SidebarProvider>
      <LayoutContent />
    </SidebarProvider>
  )
}
