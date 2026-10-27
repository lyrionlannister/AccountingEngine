import type { ReactNode } from 'react'
import type { DropdownMenuItem } from '../types/dropdown-menu'

export type DropdownMenuProps = {
  trigger: ReactNode
  items: DropdownMenuItem[]
  'aria-label': string
  align?: 'left' | 'right'
}
