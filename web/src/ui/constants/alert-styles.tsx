import type { ReactNode } from 'react'
import { AlertTriangle, CheckCircle2, Clock, Info } from 'lucide-react'
import type { AlertTone } from '../types/alert'

export const ALERT_TONE_CLASSES: Record<AlertTone, { border: string; bg: string; icon: string }> = {
  info: { border: 'border-secondary/35', bg: 'bg-secondary/8', icon: 'text-link' },
  success: { border: 'border-success/35', bg: 'bg-success/8', icon: 'text-success-text' },
  warning: { border: 'border-warning/35', bg: 'bg-warning/8', icon: 'text-warning-text' },
  error: { border: 'border-error/35', bg: 'bg-error/8', icon: 'text-error-text' },
}

export const ALERT_TONE_ICON: Record<AlertTone, ReactNode> = {
  info: <Info className="size-4" aria-hidden />,
  success: <CheckCircle2 className="size-4.5" aria-hidden />,
  warning: <Clock className="size-4" aria-hidden />,
  error: <AlertTriangle className="size-4" aria-hidden />,
}
