import type { ReactNode } from 'react'
import type { BadgeTone } from '../types/badge'

export type BadgeProps = {
  tone?: BadgeTone
  icon?: ReactNode
  children: ReactNode
  className?: string
}
