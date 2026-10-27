import type { ReactNode } from 'react'

export type DropdownMenuItem =
  | 'divider'
  | {
      label: string
      icon?: ReactNode
      kbd?: string
      danger?: boolean
      onSelect?: () => void
    }
