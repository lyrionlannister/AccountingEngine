import { ChevronDown } from 'lucide-react'
import type { SelectProps } from '../props/select-props'

export const Select = ({ className = '', children, ...rest }: SelectProps) => {
  return (
    <div className="relative">
      <select
        className={
          'min-h-8.5 w-full appearance-none rounded border border-line bg-surface px-2.5 py-1.5 pr-7 font-body text-sm text-text ' +
          'hover:border-faint focus:border-primary focus:outline-none focus:ring-[3px] focus:ring-ring ' +
          `disabled:cursor-not-allowed disabled:bg-bg disabled:text-faint ${className}`
        }
        {...rest}
      >
        {children}
      </select>
      <ChevronDown className="pointer-events-none absolute top-1/2 right-2.5 size-3.5 -translate-y-1/2 text-muted" aria-hidden />
    </div>
  )
}
