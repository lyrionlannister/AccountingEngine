import { FIELD_HELP_ICON, FIELD_HELP_TONE_CLASSES } from '../constants/field-styles'
import type { FieldProps } from '../props/field-props'

export const Field = ({ id, label, optional, help, tone = 'default', children, className = '' }: FieldProps) => {
  return (
    <div className={`grid min-w-0 content-start gap-1.5 ${className}`}>
      <label htmlFor={id} className="flex justify-between text-[12.5px] font-medium text-text">
        {label}
        {optional && <em className="font-normal text-faint not-italic">opcional</em>}
      </label>
      {children}
      {help && (
        <span className={`flex items-center gap-1.5 text-[11.5px] ${FIELD_HELP_TONE_CLASSES[tone]}`}>
          {FIELD_HELP_ICON[tone]}
          {help}
        </span>
      )}
    </div>
  )
}
