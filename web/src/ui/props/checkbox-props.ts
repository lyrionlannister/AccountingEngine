import type { InputHTMLAttributes, ReactNode } from 'react'

export type CheckboxProps = InputHTMLAttributes<HTMLInputElement> & {
  children: ReactNode
}
