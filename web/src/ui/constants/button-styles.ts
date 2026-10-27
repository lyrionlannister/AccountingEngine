import type { ButtonSize, ButtonVariant } from '../types/button'

export const BUTTON_BASE =
  'inline-flex items-center justify-center gap-1.5 whitespace-nowrap rounded font-body font-medium ' +
  'border transition-colors focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-ring ' +
  'focus-visible:border-primary disabled:cursor-not-allowed disabled:opacity-45'

export const BUTTON_VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'bg-primary border-primary text-on-primary hover:opacity-90',
  secondary: 'bg-surface border-line text-text hover:bg-bg',
  ghost: 'bg-transparent border-transparent text-text hover:bg-muted/10',
  danger: 'bg-error border-error text-on-error hover:opacity-90',
  'danger-secondary': 'bg-transparent border-error/45 text-error-text hover:bg-error/10',
  link: 'h-auto min-h-0 border-transparent bg-transparent p-0 text-link underline decoration-link/40 underline-offset-[3px] hover:opacity-80',
}

export const BUTTON_SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'h-7 px-2.5 text-xs',
  md: 'h-8.5 px-3 text-sm',
  lg: 'h-10 px-4 text-sm',
}

export const BUTTON_ICON_SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'h-7 w-7 p-0',
  md: 'h-8.5 w-8.5 p-0',
  lg: 'h-10 w-10 p-0',
}
