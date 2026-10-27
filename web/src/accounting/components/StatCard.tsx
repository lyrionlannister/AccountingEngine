import { TrendingUp } from 'lucide-react'
import { Badge } from '../../ui/components/Badge'
import type { StatCardProps } from '../props/stat-card-props'

export const StatCard = ({ label, code, value, trend }: StatCardProps) => {
  return (
    <div className="grid gap-2.5 rounded-md border border-line bg-surface p-4.5 shadow-card">
      <div className="flex items-center justify-between text-[12.5px] font-medium text-muted">
        {label}
        <code className="font-mono text-[10.5px] text-faint">{code}</code>
      </div>
      <div className="font-head text-2xl font-semibold tracking-tight tabular-nums text-text">{value}</div>
      <div className="flex items-center gap-1.5 text-xs text-muted">
        <Badge tone="success" icon={<TrendingUp className="size-3" aria-hidden />}>
          {trend}
        </Badge>
      </div>
    </div>
  )
}
