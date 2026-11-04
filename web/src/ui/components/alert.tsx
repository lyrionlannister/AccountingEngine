import { ALERT_TONE_CLASSES, ALERT_TONE_ICON } from '../constants/alert-styles'
import type { AlertProps } from '../props/alert-props'

export const Alert = ({ tone = 'info', title, children, action, className = '' }: AlertProps) => {
  const t = ALERT_TONE_CLASSES[tone]

  return (
    <div
      role={tone === 'error' ? 'alert' : undefined}
      className={`flex items-start gap-2.5 rounded border bg-surface px-3 py-2.5 text-[12.5px] text-text ${t.border} ${t.bg} ${className}`}
    >
      <span className={`mt-px shrink-0 ${t.icon}`}>{ALERT_TONE_ICON[tone]}</span>
      <div className="flex-1">
        {title && <b className="font-semibold">{title} </b>}
        {children}
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  )
}
