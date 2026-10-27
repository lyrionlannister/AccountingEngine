import type { ReactNode } from 'react'

export type FieldProps = {
  id: string
  label: string
  optional?: boolean
  help?: ReactNode
  tone?: 'default' | 'error' | 'success' | 'info'
  children: ReactNode
  className?: string
}
