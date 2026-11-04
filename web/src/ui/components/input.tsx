import type { InputProps } from '../props/input-props'

export const Input = ({ mono = false, numeric = false, invalid = false, className = '', ...rest }: InputProps) => {
  return (
    <input
      className={
        'min-h-8.5 w-full rounded border bg-surface px-2.5 py-1.5 font-body text-sm text-text ' +
        'placeholder:text-faint hover:border-faint focus:border-primary focus:outline-none focus:ring-[3px] focus:ring-ring ' +
        'disabled:cursor-not-allowed disabled:bg-bg disabled:text-faint ' +
        (invalid ? 'border-error focus:ring-error/25' : 'border-line') +
        (mono ? ' font-mono text-[12.5px]' : '') +
        (numeric ? ' text-right tabular-nums' : '') +
        ` ${className}`
      }
      aria-invalid={invalid || undefined}
      {...rest}
    />
  )
}
