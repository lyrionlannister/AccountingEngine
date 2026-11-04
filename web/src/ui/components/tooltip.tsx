import type { TooltipProps } from '../props/tooltip-props'

export const Tooltip = ({ label, children }: TooltipProps) => {
  return (
    <span className="group relative inline-flex">
      <span className="pointer-events-none absolute top-1/2 left-full z-10 ml-2.5 -translate-y-1/2 rounded bg-text px-2.5 py-1.5 font-body text-[11.5px] whitespace-nowrap text-bg opacity-0 transition-opacity group-hover:opacity-100 group-focus-within:opacity-100">
        {label}
      </span>
      {children}
    </span>
  )
}
