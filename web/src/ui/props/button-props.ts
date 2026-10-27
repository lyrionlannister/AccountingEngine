import type { ButtonHTMLAttributes, ReactNode } from 'react'
import type { ButtonSize, ButtonVariant } from '../types/button'

export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  size?: ButtonSize
  iconOnly?: boolean
  loading?: boolean
  children?: ReactNode
}
