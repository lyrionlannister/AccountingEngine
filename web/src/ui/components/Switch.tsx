import type { SwitchProps } from '../props/switch-props'

export const Switch = ({ children, className = '', ...rest }: SwitchProps) => {
  return (
    <label className={`inline-flex cursor-pointer items-center gap-2.5 font-body text-sm text-text ${className}`}>
      <input type="checkbox" className="peer sr-only" {...rest} />
      <span
        className={
          'relative h-4.5 w-8 shrink-0 rounded-full bg-faint transition-colors ' +
          'peer-checked:bg-primary peer-focus-visible:ring-[3px] peer-focus-visible:ring-ring ' +
          'after:absolute after:top-0.5 after:left-0.5 after:size-3.5 after:rounded-full after:bg-surface after:transition-transform ' +
          'peer-checked:after:translate-x-3.5 peer-checked:after:bg-on-primary'
        }
      />
      {children}
    </label>
  )
}
