import type { ReactNode } from 'react'

export type EmptyStateProps = {
  icon: ReactNode
  title: string
  description?: string
  items?: string[]
  actions?: ReactNode
}
