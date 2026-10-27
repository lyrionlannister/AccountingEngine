import type { TextareaProps } from '../props/textarea-props'

export const Textarea = ({ className = '', ...rest }: TextareaProps) => {
  return (
    <textarea
      className={
        'min-h-16 w-full resize-y rounded border border-line bg-surface px-2.5 py-1.5 font-body text-sm text-text ' +
        'placeholder:text-faint hover:border-faint focus:border-primary focus:outline-none focus:ring-[3px] focus:ring-ring ' +
        `disabled:cursor-not-allowed disabled:bg-bg disabled:text-faint ${className}`
      }
      {...rest}
    />
  )
}
