import type { SegmentedOption } from '../types/segmented-control'

export type SegmentedControlProps = {
  options: SegmentedOption[]
  value: string
  onChange: (value: string) => void
  'aria-label': string
  className?: string
}
