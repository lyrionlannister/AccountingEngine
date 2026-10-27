import type { InputHTMLAttributes, ReactNode } from 'react'

export type AffixProps = InputHTMLAttributes<HTMLInputElement> & {
  prefix?: ReactNode
  suffix?: ReactNode
  align?: 'left' | 'right'
}
