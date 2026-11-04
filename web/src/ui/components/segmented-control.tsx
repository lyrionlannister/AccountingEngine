import type { SegmentedControlProps } from '../props/segmented-control-props'

export const SegmentedControl = ({ options, value, onChange, className = '', ...rest }: SegmentedControlProps) => {
  return (
    <div role="group" className={`inline-flex gap-0.5 rounded-md border border-line bg-surface p-0.5 ${className}`} {...rest}>
      {options.map((opt) => {
        const pressed = opt.value === value
        return (
          <button
            key={opt.value}
            type="button"
            aria-pressed={pressed}
            onClick={() => onChange(opt.value)}
            className={
              'rounded-sm px-3.5 py-2 font-body text-[13px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-[1px] focus-visible:ring-primary ' +
              (pressed ? 'bg-text text-bg' : 'text-muted hover:text-text')
            }
          >
            {opt.label}
          </button>
        )
      })}
    </div>
  )
}
