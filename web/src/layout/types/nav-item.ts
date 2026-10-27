import type { LucideIcon } from 'lucide-react'

export type NavItem = {
  key: string
  label: string
  to: string
  icon: LucideIcon
  group: string
  zone?: boolean
}
