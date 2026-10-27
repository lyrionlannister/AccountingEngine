import type { ReactNode } from 'react'
import type { SortDirection, TableColumn } from '../types/table'

export type TableProps<T> = {
  columns: TableColumn<T>[]
  rows: T[]
  rowKey: (row: T) => string | number
  sortKey?: string
  sortDir?: SortDirection
  onSort?: (key: string) => void
  empty?: ReactNode
}
