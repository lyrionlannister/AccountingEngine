import type { InputHTMLAttributes } from 'react'

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  mono?: boolean
  numeric?: boolean
  invalid?: boolean
}
