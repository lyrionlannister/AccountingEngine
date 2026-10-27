import type { ReactNode } from 'react'

export type DialogProps = {
  open: boolean
  onClose: () => void
  title: ReactNode
  description?: ReactNode
  children?: ReactNode
  footer?: ReactNode
}
