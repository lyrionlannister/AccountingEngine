import type { CheckboxProps } from '../props/checkbox-props'

export const Checkbox = ({ children, className = '', disabled, ...rest }: CheckboxProps) => {
  return (
    <label className={`inline-flex items-center gap-2 font-body text-sm text-text ${disabled ? 'opacity-50' : 'cursor-pointer'} ${className}`}>
      <input type="checkbox" disabled={disabled} className="size-4 cursor-pointer accent-primary disabled:cursor-not-allowed" {...rest} />
      {children}
    </label>
  )
}
