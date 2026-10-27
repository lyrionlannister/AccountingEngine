import { BookText, Layers, LayoutGrid } from 'lucide-react'
import type { NavItem } from '../types/nav-item'

export const NAV_ITEMS: NavItem[] = [
  { key: 'panel', label: 'Panel', to: '/panel', icon: LayoutGrid, group: 'General' },
  { key: 'polizas', label: 'Pólizas', to: '/polizas', icon: BookText, group: 'Contabilidad', zone: true },
  { key: 'componentes', label: 'Componentes', to: '/componentes', icon: Layers, group: 'Sistema' },
]

export const NAV_GROUP_START_KEYS = new Set(
  NAV_ITEMS.filter((item, i, all) => i === 0 || all[i - 1].group !== item.group).map((item) => item.key),
)
