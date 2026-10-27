import { BADGE_DEFAULT_ICON, BADGE_TONE_CLASSES } from '../constants/badge-styles'
import type { BadgeProps } from '../props/badge-props'

export const Badge = ({ tone = 'neutral', icon, children, className = '' }: BadgeProps) => {
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-sm px-1.5 py-0.5 font-body text-[11.5px] font-medium whitespace-nowrap ${BADGE_TONE_CLASSES[tone]} ${className}`}
    >
      {icon ?? BADGE_DEFAULT_ICON[tone]}
      {children}
    </span>
  )
}
