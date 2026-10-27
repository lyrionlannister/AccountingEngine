import type { TabsProps } from '../props/tabs-props'

export const Tabs = ({ tabs, value, onChange, className = '' }: TabsProps) => {
  return (
    <div role="tablist" className={`flex gap-1 border-b border-line ${className}`}>
      {tabs.map((tab) => {
        const selected = tab.key === value
        return (
          <button
            key={tab.key}
            type="button"
            role="tab"
            aria-selected={selected}
            onClick={() => onChange(tab.key)}
            className={
              '-mb-px inline-flex items-center gap-1.5 border-b-2 px-2.5 pt-2.5 pb-2.75 font-body text-sm font-medium ' +
              (selected ? 'border-primary text-text' : 'border-transparent text-muted hover:text-text')
            }
          >
            {tab.label}
            {tab.count != null && (
              <span className="rounded-sm bg-muted/14 px-1.5 py-0.5 font-mono text-[10.5px] font-medium">{tab.count}</span>
            )}
          </button>
        )
      })}
    </div>
  )
}
