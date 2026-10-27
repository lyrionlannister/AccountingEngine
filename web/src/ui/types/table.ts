import type { ReactNode } from 'react'

export type SortDirection = 'asc' | 'desc'

export type TableColumn<T> = {
  key: string
  header: string
  align?: 'left' | 'right'
  sortable?: boolean
  render: (row: T) => ReactNode
}
