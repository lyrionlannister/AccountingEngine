import type { ReactNode } from 'react'
import type { AlertTone } from '../types/alert'

export type AlertProps = {
  tone?: AlertTone
  title?: ReactNode
  children?: ReactNode
  action?: ReactNode
  className?: string
}
