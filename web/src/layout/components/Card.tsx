import type { CardProps } from '../props/card-props'

export const Card = ({ title, description, children }: CardProps) => {
  return (
    <div className="grid gap-4 rounded-md border border-line bg-surface p-4.5 shadow-card">
      <div>
        <h2 className="font-head text-[15px] font-semibold text-text">{title}</h2>
        {description && <p className="text-xs text-muted">{description}</p>}
      </div>
      {children}
    </div>
  )
}
