import { Loader2 } from 'lucide-react'
import { BUTTON_BASE, BUTTON_ICON_SIZE_CLASSES, BUTTON_SIZE_CLASSES, BUTTON_VARIANT_CLASSES } from '../constants/button-styles'
import type { ButtonProps } from '../props/button-props'

export const Button = ({
  variant = 'secondary',
  size = 'md',
  iconOnly = false,
  loading = false,
  disabled,
  className = '',
  children,
  ...rest
}: ButtonProps) => {
  const isLink = variant === 'link'
  const sizing = isLink ? '' : iconOnly ? BUTTON_ICON_SIZE_CLASSES[size] : BUTTON_SIZE_CLASSES[size]

  return (
    <button
      type="button"
      className={`${BUTTON_BASE} ${BUTTON_VARIANT_CLASSES[variant]} ${sizing} ${className}`}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading && <Loader2 className="size-3.5 animate-spin" aria-hidden />}
      {children}
    </button>
  )
}
