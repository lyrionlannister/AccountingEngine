import type { ReactNode } from 'react'
import { AlertTriangle, CheckCircle2, Info } from 'lucide-react'
import type { FieldProps } from '../props/field-props'

export const FIELD_HELP_TONE_CLASSES: Record<Required<FieldProps>['tone'], string> = {
  default: 'text-muted',
  error: 'text-error-text',
  success: 'text-success-text',
  info: 'text-muted',
}

export const FIELD_HELP_ICON: Record<Required<FieldProps>['tone'], ReactNode> = {
  default: null,
  error: <AlertTriangle className="size-3" aria-hidden />,
  success: <CheckCircle2 className="size-3" aria-hidden />,
  info: <Info className="size-3" aria-hidden />,
}
