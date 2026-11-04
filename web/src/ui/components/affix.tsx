import type { AffixProps } from '../props/affix-props'

export const Affix = ({ prefix, suffix, align = 'right', className = '', ...rest }: AffixProps) => {
  return (
    <div className="flex min-h-8.5 w-full items-center gap-2 rounded border border-line bg-surface px-2.5 has-[input:focus]:border-primary has-[input:focus]:ring-[3px] has-[input:focus]:ring-ring hover:border-faint">
      {prefix && <span className="text-[12.5px] text-muted">{prefix}</span>}
      <input
        className={`min-w-0 flex-1 border-0 bg-transparent py-1.5 font-body text-sm text-text outline-none placeholder:text-faint ${
          align === 'right' ? 'text-right' : 'text-left'
        } ${className}`}
        {...rest}
      />
      {suffix && <span className="text-[12.5px] text-muted">{suffix}</span>}
    </div>
  )
}
