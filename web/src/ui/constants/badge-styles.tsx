import type { ReactNode } from 'react'
import { AlertTriangle, CheckCircle2, Clock } from 'lucide-react'
import type { BadgeTone } from '../types/badge'

export const BADGE_TONE_CLASSES: Record<BadgeTone, string> = {
  success: 'text-success-text bg-success/14',
  warning: 'text-warning-text bg-warning/16',
  error: 'text-error-text bg-error/14',
  neutral: 'text-muted bg-muted/12',
}

export const BADGE_DEFAULT_ICON: Record<BadgeTone, ReactNode> = {
  success: <CheckCircle2 className="size-3" aria-hidden />,
  warning: <Clock className="size-3" aria-hidden />,
  error: <AlertTriangle className="size-3" aria-hidden />,
  neutral: null,
}
